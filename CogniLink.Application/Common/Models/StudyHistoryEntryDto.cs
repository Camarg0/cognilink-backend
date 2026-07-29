namespace CogniLink.Application.Common.Models;

public sealed record StudyHistoryEntryDto(
    DateOnly Date,
    int AnswersCount,
    int CorrectCount,
    int IncorrectCount,
    long TotalTimeSeconds);
