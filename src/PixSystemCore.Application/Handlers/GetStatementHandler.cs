using MediatR;
using Microsoft.Extensions.Logging;
using PixSystemCore.Api.Queries.GetStatement;
using PixSystemCore.Domain.Exceptions;
using PixSystemCore.Domain.Interfaces;

namespace PixSystemCore.Application.Handlers;

public class GetStatementHandler : IRequestHandler<GetStatementQuery, GetStatementResponse>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<GetStatementHandler> _logger;

    public GetStatementHandler(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        ILogger<GetStatementHandler> logger)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task<GetStatementResponse> Handle(GetStatementQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing statement query for AccountId: {AccountId}", request.AccountId);

        var account = await _accountRepository.GetByIdAsync(request.AccountId);
        if (account == null)
            throw new AccountNotFoundException("Conta não encontrada");

        var transactions = await _transactionRepository.GetByAccountAsync(request.AccountId, 1, 50, cancellationToken);

        var transactionItems = transactions.Select(t =>
        {
            bool isDebit = t.SourceAccountId == account.Id;

            return new StatementItemDto
            {
                TransactionId = t.Id,
                Type = isDebit ? "DEBIT" : "CREDIT",
                Amount = t.Amount,
                CounterpartyName = isDebit
                    ? t.TargetAccount?.HolderName ?? "Unknown Destination"
                    : t.SourceAccount?.HolderName ?? "Unknown Source",
                CounterpartyPixKey = isDebit
                    ? t.TargetAccount?.PixKey ?? string.Empty
                    : t.SourceAccount?.PixKey ?? string.Empty,
                CreatedAt = t.CreatedAt,
                ProcessedAt = t.ProcessedAt,
                Status = t.Status.ToString()
            };
        }).ToList();

        return new GetStatementResponse
        {
            AccountId = account.Id,
            HolderName = account.HolderName,
            PixKey = account.PixKey,
            Balance = account.Balance,
            BlockedBalance = account.BlockedBalance,
            AvailableBalance = account.AvailableBalance,
            CreatedAt = account.CreatedAt,
            Transactions = transactionItems
        };
    }
}
