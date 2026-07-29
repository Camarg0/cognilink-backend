using CogniLink.Application.Common.Models;
using CogniLink.Application.Dashboard.Queries.GetDashboard;
using CogniLink.Application.Dashboard.Queries.GetDeckStatistics;
using CogniLink.Application.Dashboard.Queries.GetStudyHistory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CogniLink.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDashboardQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("decks/{deckId}/statistics")]
    [ProducesResponseType(typeof(DeckStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDeckStatistics(string deckId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDeckStatisticsQuery(deckId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("analytics/study-history")]
    [ProducesResponseType(typeof(IReadOnlyList<StudyHistoryEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStudyHistory([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStudyHistoryQuery(from, to), cancellationToken);
        return Ok(result);
    }
}
