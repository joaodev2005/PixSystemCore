namespace PixSystemCore.Api.Queries.GetStatement;

public class StatementItemDto
{
    public Guid TransactionId { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string CounterpartyName { get; init; } = string.Empty;
    public string CounterpartyPixKey { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public string Status { get; init; } = string.Empty;
}
