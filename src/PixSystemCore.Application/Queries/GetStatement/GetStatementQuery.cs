using MediatR;

namespace PixSystemCore.Api.Queries.GetStatement;

public record GetStatementQuery(Guid AccountId) : IRequest<GetStatementResponse>;