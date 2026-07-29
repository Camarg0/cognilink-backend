namespace CogniLink.Application.Common.Models;

public sealed record DeckStatisticsDto(
    string DeckId,
    int TotalFlashcards,
    int DueFlashcardsCount,
    double DeckMasteryPercentage,
    IReadOnlyList<PerformanceGroupDto> PerformanceByDifficulty,
    IReadOnlyList<PerformanceGroupDto> PerformanceBySubarea,
    IReadOnlyList<PerformanceGroupDto> PerformanceByType);
