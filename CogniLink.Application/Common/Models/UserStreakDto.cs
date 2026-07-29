namespace CogniLink.Application.Common.Models;

public sealed record UserStreakDto(
    int CurrentStreak,
    int LongestStreak,
    DateOnly? LastStudyDate);