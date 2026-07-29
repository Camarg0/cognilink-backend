using CogniLink.Api.Contracts;
using CogniLink.Application.Common.Models;
using CogniLink.Application.StudySessions.Commands.CancelStudySession;
using CogniLink.Application.StudySessions.Commands.CompleteStudySession;
using CogniLink.Application.StudySessions.Commands.StartStudySession;
using CogniLink.Application.StudySessions.Commands.SubmitAnswer;
using CogniLink.Application.StudySessions.Queries.GetDueFlashcards;
using CogniLink.Application.StudySessions.Queries.GetNextCard;
using CogniLink.Application.StudySessions.Queries.GetUserStreak;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CogniLink.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class StudySessionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StudySessionsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("study-sessions")]
    [ProducesResponseType(typeof(StudySessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> StartSession(StartStudySessionRequest request, CancellationToken cancellationToken)
    {
        var command = new StartStudySessionCommand(request.DeckId);
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("study-sessions/{sessionId}/next-card")]
    [ProducesResponseType(typeof(NextStudyCardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetNextCard(string sessionId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetNextCardQuery(sessionId), cancellationToken);
        return result is null ? NoContent() : Ok(result);
    }

    [HttpPost("study-sessions/{sessionId}/answers")]
    [ProducesResponseType(typeof(StudyAnswerResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitAnswer(string sessionId, SubmitStudyAnswerRequest request, CancellationToken cancellationToken)
    {
        var command = new SubmitAnswerCommand(
            sessionId,
            request.AttemptId,
            request.IsCorrect,
            request.TimeToAnswerSeconds,
            request.HintsViewed);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("study-sessions/{sessionId}/complete")]
    [ProducesResponseType(typeof(StudySessionSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CompleteSession(string sessionId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CompleteStudySessionCommand(sessionId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("study-sessions/{sessionId}/cancel")]
    [ProducesResponseType(typeof(StudySessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelSession(string sessionId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelStudySessionCommand(sessionId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("reviews/due")]
    [ProducesResponseType(typeof(IReadOnlyList<DueFlashcardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDueFlashcards(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDueFlashcardsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("users/me/streak")]
    [ProducesResponseType(typeof(UserStreakDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserStreak(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserStreakQuery(), cancellationToken);
        return Ok(result);
    }
}
