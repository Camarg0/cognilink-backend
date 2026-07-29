namespace CogniLink.Application.Common.Models;

public sealed record DashboardSummaryDto(
    double MasteryPercentage,
    long TotalStudyTimeSeconds,
    int CompletedCardsCount,
    double RetentionRate,
    int CurrentStreak,
    int LongestStreak,
    int DueFlashcardsCount);
