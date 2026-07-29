using CogniLink.Domain.Enums;

namespace CogniLink.Application.Common.Models;

public sealed record NextStudyCardDto(
    string SessionId,
    string FlashcardId,
    Guid AttemptId,
    FlashcardType Type,
    FlashcardDifficulty Difficulty,
    string? Question,
    string? Answer,
    string? ClozeText,
    IReadOnlyList<FlashcardAlternativeDto>? Alternatives,
    IReadOnlyList<string>? Hints,
    string DueState);