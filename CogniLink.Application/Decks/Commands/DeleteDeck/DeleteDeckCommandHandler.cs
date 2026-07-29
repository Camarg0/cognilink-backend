using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using MediatR;

namespace CogniLink.Application.Decks.Commands.DeleteDeck;

public sealed class DeleteDeckCommandHandler : IRequestHandler<DeleteDeckCommand>
{
    private readonly IDeckRepository _deckRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteDeckCommandHandler(IDeckRepository deckRepository, ICurrentUserService currentUserService)
    {
        _deckRepository = deckRepository;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteDeckCommand request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUserService.UserId ?? throw new AuthenticationFailedException();

        var deck = await _deckRepository.GetByIdAsync(request.Id, cancellationToken);
        if (deck is null || deck.OwnerId != ownerId)
        {
            throw new NotFoundException("Baralho não encontrado.");
        }

        await _deckRepository.DeleteAsync(deck.Id, cancellationToken);
    }
}
