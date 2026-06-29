namespace PixSystemCore.Domain.Events;

public class PixRequestedEvent
{
    public Guid TransactionId { get; init; }
    public Guid SourceAccountId { get; init; }
    public Guid DestinationAccountId { get; init; }
    public decimal Amount { get; init; }
    public string IdempotencyKey { get; init; } = string.Empty;
    public DateTime RequestedAt { get; init; }
    public string? TraceId { get; init; }
}
