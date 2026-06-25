using PixSystemCore.Domain.Enums;

namespace PixSystemCore.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public Guid SourceAccountId { get; private set; }
    public Guid TargetAccountId { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string IdempotencyKey { get; private set; }

    public Account SourceAccount { get; private set; }
    public Account TargetAccount { get; private set; }

    private Transaction() { }

    public Transaction(Guid sourceAccountId, Guid targetAccountId, decimal amount, string idempotencyKey)
    {
        Id = Guid.NewGuid();
        SourceAccountId = sourceAccountId;
        TargetAccountId = targetAccountId;
        Amount = amount;
        Status = TransactionStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        IdempotencyKey = idempotencyKey;
    }

    public void MarkAsProcessed()
    {
        Status = TransactionStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string reason)
    {
        Status = TransactionStatus.Failed;
        ErrorMessage = reason;
        ProcessedAt = DateTime.UtcNow;
    }
}
