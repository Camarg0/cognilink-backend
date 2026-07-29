namespace CogniLink.Application.Common.Models;

public sealed record StudyAnswerResultDto(
    string SessionId,
    string FlashcardId,
    Guid AttemptId,
    bool WasIdempotentReplay,
    bool IsCorrect,
    DateTime AnsweredAt,
    double EaseFactor,
    int IntervalDays,
    DateTime? NextReviewDate,
    int Repetitions,
    int Lapses,
    DateTime? LastReviewedAt);