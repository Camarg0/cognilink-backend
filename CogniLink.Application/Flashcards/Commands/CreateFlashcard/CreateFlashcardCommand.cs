using CogniLink.Application.Common.Models;
using CogniLink.Domain.Enums;
using MediatR;

namespace CogniLink.Application.Flashcards.Commands.CreateFlashcard;

public sealed record CreateFlashcardCommand(
    string DeckId,
    FlashcardType Type,
    FlashcardDifficulty Difficulty,
    string? Subarea,
    List<string>? Hints,
    string? Question,
    string? Answer,
    string? ClozeText,
    List<string>? ValidAnswers,
    List<FlashcardAlternativeInput>? Alternatives) : IRequest<FlashcardDto>;