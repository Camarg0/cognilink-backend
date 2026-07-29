namespace CogniLink.Application.Common.Models;

public sealed record DueFlashcardDto(
    string FlashcardId,
    string DeckId,
    DateTime? NextReviewDate,
    double EaseFactor,
    int IntervalDays,
    int Repetitions,
    int Lapses);