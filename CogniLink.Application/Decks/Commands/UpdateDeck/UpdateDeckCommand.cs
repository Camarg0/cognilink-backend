using CogniLink.Application.Common.Models;
using CogniLink.Domain.Enums;
using MediatR;

namespace CogniLink.Application.Decks.Commands.UpdateDeck;

public sealed record UpdateDeckCommand(
    string Id,
    string Name,
    string? Description,
    DeckDifficulty Difficulty,
    List<string>? Categories) : IRequest<DeckResponse>;
