using MediatR;

namespace CogniLink.Application.Decks.Commands.DeleteDeck;

public sealed record DeleteDeckCommand(string Id) : IRequest;
