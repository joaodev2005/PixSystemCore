using MediatR;
using Microsoft.AspNetCore.Mvc;
using PixSystemCore.Api.Queries.GetStatement;
using PixSystemCore.Application.Commands;

namespace PixSystemCore.Api.Controllers;

[ApiController]
[Route("api/contas")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> OpenAccount([FromBody] OpenAccountCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetStatement), new { id = result.AccountId }, result);
    }

    [HttpPost("pix")]
    public async Task<IActionResult> RequestPix(
        [FromBody] ExecutePixCommand command,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
    {
        command.IdempotencyKey = idempotencyKey;
        var result = await _mediator.Send(command);
        return Accepted(result);
    }

    [HttpGet("{id}/statement")]
    public async Task<IActionResult> GetStatement(Guid id)
    {
        var query = new GetStatementQuery(id);

        var result = await _mediator.Send(query);

        return Ok(result);
    }
}
