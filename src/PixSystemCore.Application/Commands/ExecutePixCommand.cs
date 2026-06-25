using MediatR;

namespace PixSystemCore.Application.Commands;

public class ExecutePixCommand : IRequest<ExecutePixResponse>
{
    public Guid SourceAccountId { get; init; }
    public string TargetPixKey { get; init; }
    public decimal Amount { get; init; }
    public string IdempotencyKey { get; init; }
}

public record ExecutePixResponse
{
    public Guid TransactionId { get; init; }
    public string Status { get; init; }
    public string Message { get; init; }
}