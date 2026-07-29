using CogniLink.Domain.Enums;

namespace CogniLink.Application.Common.Models;

public sealed record StudySessionSummaryDto(
    string SessionId,
    StudySessionStatus Status,
    int CardsStudied,
    int CorrectAnswers,
    double AccuracyRate,
    int DurationSeconds,
    DateTime StartedAt,
    DateTime CompletedAt);