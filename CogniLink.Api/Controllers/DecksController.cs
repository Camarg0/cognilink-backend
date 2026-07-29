using CogniLink.Api.Contracts;
using CogniLink.Application.Decks.Commands.CreateDeck;
using CogniLink.Application.Decks.Commands.DeleteDeck;
using CogniLink.Application.Decks.Commands.UpdateDeck;
using CogniLink.Application.Decks.Queries.GetDeckById;
using CogniLink.Application.Decks.Queries.ListDecks;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CogniLink.Api.Controllers;

[ApiController]
[Route("api/decks")]
[Authorize]
public sealed class DecksController : ControllerBase
{
    private readonly IMediator _mediator;

    public DecksController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create(CreateDeckRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request.Adapt<CreateDeckCommand>(), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet]
    public async Task<IActionResult> List(string? search, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ListDecksQuery(search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDeckByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateDeckRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateDeckCommand(id, request.Name, request.Description, request.Difficulty, request.Categories);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteDeckCommand(id), cancellationToken);
        return NoContent();
    }
}
