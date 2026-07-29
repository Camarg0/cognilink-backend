using CogniLink.Application.Common.Models;
using CogniLink.Domain.Enums;
using MediatR;

namespace CogniLink.Application.Flashcards.Commands.UpdateFlashcard;

public sealed record UpdateFlashcardCommand(
    string Id,
    FlashcardType Type,
    FlashcardDifficulty Difficulty,
    string? Subarea,
    List<string>? Hints,
    string? Question,
    string? Answer,
    string? ClozeText,
    List<string>? ValidAnswers,
    List<FlashcardAlternativeInput>? Alternatives) : IRequest<FlashcardDto>;