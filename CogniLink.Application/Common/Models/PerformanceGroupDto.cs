namespace CogniLink.Application.Common.Models;

public sealed record PerformanceGroupDto(
    string GroupKey,
    int TotalAnswers,
    int CorrectAnswers,
    double AccuracyRate);
