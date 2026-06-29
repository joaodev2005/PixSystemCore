namespace PixSystemCore.Api.Queries.GetStatement;

public class GetStatementResponse
{
    public Guid AccountId { get; init; }
    public string HolderName { get; init; } = string.Empty;
    public string PixKey { get; init; } = string.Empty;
    public decimal Balance { get; init; }
    public decimal BlockedBalance { get; init; }
    public decimal AvailableBalance { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<StatementItemDto> Transactions { get; init; } = [];
}
