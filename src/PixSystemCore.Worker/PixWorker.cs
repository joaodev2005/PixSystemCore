using System.Diagnostics;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PixSystemCore.Domain.Entities;
using PixSystemCore.Domain.Events;
using PixSystemCore.Domain.Exceptions;
using PixSystemCore.Infrastructure.Data;
using PixSystemCore.Infrastructure.Services;
using Polly;
using Polly.Retry;

namespace PixSystemCore.Worker;

public class PixWorker : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PixWorker> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public PixWorker(IConsumer<string, string> consumer,
        IServiceScopeFactory scopeFactory,
        ILogger<PixWorker> logger)
    {
        _consumer = consumer;
        _scopeFactory = scopeFactory;
        _logger = logger;

        _retryPolicy = Policy
            .Handle<SqlException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 100),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception,
                        "Tentativa {RetryCount} de processamento. Aguardando {TimeSpan}ms",
                        retryCount, timeSpan.TotalMilliseconds);
                });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("pix-solicitado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(stoppingToken);

                if (result?.Message == null) continue;

                var pixEvent = JsonSerializer.Deserialize<PixRequestedEvent>(result.Message.Value);

                if (pixEvent != null)
                {
                    await ProcessPixWithResilienceAsync(pixEvent);
                }

                _consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consumir mensagem do Kafka");
            }
        }
    }

    private async Task ProcessPixWithResilienceAsync(PixRequestedEvent pixEvent)
    {
        using var activity = new ActivitySource("PixCore.Worker").StartActivity("ProcessPix");
        activity?.SetTag("transactionId", pixEvent.TransactionId);
        activity?.SetTag("traceId", pixEvent.TraceId);

        await _retryPolicy.ExecuteAsync(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PixSystemCoreDbContext>();
            var redisService = scope.ServiceProvider.GetRequiredService<RedisService>();

            var transacao = new Transaction(
                pixEvent.SourceAccountId,
                pixEvent.DestinationAccountId,
                pixEvent.Amount,
                pixEvent.IdempotencyKey
            );

            await using var dbTransaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var contaOrigem = await dbContext.Accounts.FindAsync(pixEvent.SourceAccountId);
                var contaDestino = await dbContext.Accounts.FindAsync(pixEvent.DestinationAccountId);

                if (contaOrigem == null || contaDestino == null)
                    throw new DomainException("Conta não encontrada");

                contaOrigem.LockBalance(pixEvent.Amount); 
                
                contaOrigem.Debit(pixEvent.Amount);
                contaDestino.Credit(pixEvent.Amount);

                transacao.MarkAsProcessed();

                dbContext.Transactions.Add(transacao);
                await dbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                _logger.LogInformation(
                    "Pix processado com sucesso: {TransactionId} - Amount: {Amount}",
                    pixEvent.TransactionId, pixEvent.Amount);
            }
            catch (DomainException domainEx)
            {
                await dbTransaction.RollbackAsync();

                await using var failTransaction = await dbContext.Database.BeginTransactionAsync();

                transacao.MarkAsFailed(domainEx.Message);
                dbContext.Transactions.Add(transacao);

                await dbContext.SaveChangesAsync();
                await failTransaction.CommitAsync();

                _logger.LogWarning("Pix falhou devido a regra de negócio: {Reason}", domainEx.Message);
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync();
                throw; 
            }
            finally
            {
                await redisService.ReleaseLockAsync($"bloqueio:{pixEvent.SourceAccountId}");
            }
        });
    }
}
