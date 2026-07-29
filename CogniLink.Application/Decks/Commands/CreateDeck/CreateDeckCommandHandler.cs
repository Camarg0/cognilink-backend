using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Models;
using CogniLink.Domain.Entities;
using MediatR;

namespace CogniLink.Application.Decks.Commands.CreateDeck;

public sealed class CreateDeckCommandHandler : IRequestHandler<CreateDeckCommand, DeckResponse>
{
    private readonly IDeckRepository _deckRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateDeckCommandHandler(IDeckRepository deckRepository, ICurrentUserService currentUserService)
    {
        _deckRepository = deckRepository;
        _currentUserService = currentUserService;
    }

    public async Task<DeckResponse> Handle(CreateDeckCommand request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUserService.UserId ?? throw new AuthenticationFailedException();

        var deck = Deck.Create(
            Guid.NewGuid().ToString("N"),
            ownerId,
            request.Name,
            request.Description,
            request.Difficulty,
            request.Categories);

        await _deckRepository.AddAsync(deck, cancellationToken);

        return new DeckResponse(
            deck.Id,
            deck.Name,
            deck.Description,
            deck.Difficulty.ToString(),
            deck.Categories,
            0,
            deck.CreatedAt,
            deck.UpdatedAt);
    }
}
