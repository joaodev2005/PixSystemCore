using MediatR;
using Microsoft.Extensions.Logging;
using PixSystemCore.Application.Commands;
using PixSystemCore.Domain.Events;
using PixSystemCore.Domain.Exceptions;
using PixSystemCore.Domain.Interfaces;

namespace PixSystemCore.Application.Handlers;

public class ExecutePixHandler : IRequestHandler<ExecutePixCommand, ExecutePixResponse>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IRedisService _redisService;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly ILogger<ExecutePixHandler> _logger;

    public ExecutePixHandler(
        IAccountRepository accountRepository,
        IRedisService redisService,
        IKafkaProducer kafkaProducer,
        ILogger<ExecutePixHandler> logger)
    {
        _accountRepository = accountRepository;
        _redisService = redisService;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
    }
    public async Task<ExecutePixResponse> Handle(ExecutePixCommand request, CancellationToken cancellationToken)
    {
        var cachedResponse = await _redisService.GetIdempotentResponseAsync<ExecutePixResponse>(request.IdempotencyKey);
        if (cachedResponse != null)
        {
            _logger.LogInformation("Requisição idempotente detectada: {Key}", request.IdempotencyKey);
            return cachedResponse;
        }

        var sourceAccount = await _accountRepository.GetByIdAsync(request.SourceAccountId, cancellationToken);
        if (sourceAccount == null)
            throw new AccountNotFoundException("Conta origem não encontrada");

        var targetAccount = await _accountRepository.GetByPixKeyAsync(request.TargetPixKey, cancellationToken);
        if (targetAccount == null)
            throw new PixKeyNotFoundException("Chave Pix destino não encontrada");

        if (sourceAccount.Id == targetAccount.Id)
            throw new DomainException("Não é possível transferir para a mesma conta");

        if (request.Amount <= 0)
            throw new DomainException("O valor do Pix deve ser maior que zero.");

        var lockKey = $"lock:{sourceAccount.Id}";
        var isLockAcquired = await _redisService.AcquireLockAsync(lockKey, request.Amount);

        if (!isLockAcquired)
            throw new InsufficientBalanceException("Não foi possível bloquear o saldo. Por favor, tente novamente.");

        try
        {
            var pixEvent = new PixRequestedEvent
            {
                TransactionId = Guid.NewGuid(),
                SourceAccountId = sourceAccount.Id,
                DestinationAccountId = targetAccount.Id,
                Amount = request.Amount,
                IdempotencyKey = request.IdempotencyKey,
                RequestedAt = DateTime.UtcNow,
                TraceId = System.Diagnostics.Activity.Current?.TraceId.ToString()
            };

            await _kafkaProducer.PublishPixRequestedAsync(pixEvent);

            var response = new ExecutePixResponse
            {
                TransactionId = pixEvent.TransactionId,
                Status = "PROCESSANDO",
                Message = "Pix recebido e em processamento"
            };

            await _redisService.CacheIdempotentResponseAsync(request.IdempotencyKey, response);

            return response;
        }
        catch (Exception)
        {
            await _redisService.ReleaseLockAsync(lockKey);
            throw;
        }
    }
}
