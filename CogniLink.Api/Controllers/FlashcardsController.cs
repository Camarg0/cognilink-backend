using CogniLink.Api.Contracts;
using CogniLink.Application.Common.Models;
using CogniLink.Application.Flashcards.Commands.CreateFlashcard;
using CogniLink.Application.Flashcards.Commands.DeleteFlashcard;
using CogniLink.Application.Flashcards.Commands.UpdateFlashcard;
using CogniLink.Application.Flashcards.Queries.GetFlashcardById;
using CogniLink.Application.Flashcards.Queries.ListFlashcardsByDeck;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CogniLink.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class FlashcardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FlashcardsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("decks/{deckId}/flashcards")]
    [ProducesResponseType(typeof(FlashcardDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(string deckId, CreateFlashcardRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateFlashcardCommand(
            deckId,
            request.Type,
            request.Difficulty,
            request.Subarea,
            request.Hints,
            request.Question,
            request.Answer,
            request.ClozeText,
            request.ValidAnswers,
            request.Alternatives?.Select(alternative => new FlashcardAlternativeInput(alternative.Text, alternative.IsCorrect)).ToList());

        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("decks/{deckId}/flashcards")]
    [ProducesResponseType(typeof(IReadOnlyList<FlashcardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListByDeck(string deckId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListFlashcardsByDeckQuery(deckId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("flashcards/{flashcardId}")]
    [ProducesResponseType(typeof(FlashcardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string flashcardId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFlashcardByIdQuery(flashcardId), cancellationToken);
        return Ok(result);
    }

    [HttpPut("flashcards/{flashcardId}")]
    [ProducesResponseType(typeof(FlashcardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string flashcardId, UpdateFlashcardRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateFlashcardCommand(
            flashcardId,
            request.Type,
            request.Difficulty,
            request.Subarea,
            request.Hints,
            request.Question,
            request.Answer,
            request.ClozeText,
            request.ValidAnswers,
            request.Alternatives?.Select(alternative => new FlashcardAlternativeInput(alternative.Text, alternative.IsCorrect)).ToList());

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("flashcards/{flashcardId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string flashcardId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteFlashcardCommand(flashcardId), cancellationToken);
        return NoContent();
    }
}
