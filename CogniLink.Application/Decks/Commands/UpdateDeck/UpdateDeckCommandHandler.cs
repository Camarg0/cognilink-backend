using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.Decks.Commands.UpdateDeck;

public sealed class UpdateDeckCommandHandler : IRequestHandler<UpdateDeckCommand, DeckResponse>
{
    private readonly IDeckRepository _deckRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateDeckCommandHandler(IDeckRepository deckRepository, ICurrentUserService currentUserService)
    {
        _deckRepository = deckRepository;
        _currentUserService = currentUserService;
    }

    public async Task<DeckResponse> Handle(UpdateDeckCommand request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUserService.UserId ?? throw new AuthenticationFailedException();

        var deck = await _deckRepository.GetByIdAsync(request.Id, cancellationToken);
        if (deck is null || deck.OwnerId != ownerId)
        {
            throw new NotFoundException("Baralho não encontrado.");
        }

        deck.UpdateDetails(request.Name, request.Description, request.Difficulty, request.Categories);
        await _deckRepository.UpdateAsync(deck, cancellationToken);

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
