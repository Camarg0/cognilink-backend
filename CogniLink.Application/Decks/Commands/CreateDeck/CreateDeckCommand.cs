using CogniLink.Application.Common.Models;
using CogniLink.Domain.Enums;
using MediatR;

namespace CogniLink.Application.Decks.Commands.CreateDeck;

public sealed record CreateDeckCommand(
    string Name,
    string? Description,
    DeckDifficulty Difficulty,
    List<string>? Categories) : IRequest<DeckResponse>;
