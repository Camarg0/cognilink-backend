using CogniLink.Domain.Enums;

namespace CogniLink.Application.Common.Models;

public sealed record FlashcardAlternativeDto(string Text, bool IsCorrect);

public sealed record FlashcardDto(
    string Id,
    string DeckId,
    FlashcardType Type,
    FlashcardDifficulty Difficulty,
    string? Subarea,
    IReadOnlyList<string> Hints,
    string? Question,
    string? Answer,
    string? ClozeText,
    IReadOnlyList<string>? ValidAnswers,
    IReadOnlyList<FlashcardAlternativeDto>? Alternatives,
    DateTime CreatedAt,
    DateTime UpdatedAt);