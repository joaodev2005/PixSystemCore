using MediatR;
using PixSystemCore.Domain.Enums;

namespace PixSystemCore.Application.Commands;

public class OpenAccountCommand : IRequest<OpenAccountResponse>
{
    public string HolderName { get; init; }
    public string PixKey { get; init; }
    public PixKeyType KeyType { get; init; }
}

public record OpenAccountResponse
{
    public Guid AccountId { get; init; }
    public string HolderName { get; init; }
    public string PixKey { get; init; }
    public decimal InitialBalance { get; init; }
}
