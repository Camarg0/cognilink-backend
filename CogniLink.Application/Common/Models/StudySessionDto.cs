using CogniLink.Domain.Enums;

namespace CogniLink.Application.Common.Models;

public sealed record StudySessionDto(
    string Id,
    string DeckId,
    StudySessionStatus Status,
    DateTime StartedAt,
    DateTime? CompletedAt);