using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Mappings;
using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.Flashcards.Queries.ListFlashcardsByDeck;

public sealed class ListFlashcardsByDeckQueryHandler : IRequestHandler<ListFlashcardsByDeckQuery, IReadOnlyList<FlashcardDto>>
{
    private readonly IDeckRepository _deckRepository;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly ICurrentUserService _currentUserService;

    public ListFlashcardsByDeckQueryHandler(
        IDeckRepository deckRepository,
        IFlashcardRepository flashcardRepository,
        ICurrentUserService currentUserService)
    {
        _deckRepository = deckRepository;
        _flashcardRepository = flashcardRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<FlashcardDto>> Handle(ListFlashcardsByDeckQuery request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUserService.UserId ?? throw new AuthenticationFailedException();

        var deck = await _deckRepository.GetByIdAsync(request.DeckId, cancellationToken);
        if (deck is null || deck.OwnerId != ownerId)
        {
            throw new NotFoundException("Baralho nao encontrado.");
        }

        var flashcards = await _flashcardRepository.ListByDeckIdAsync(request.DeckId, cancellationToken);
        return flashcards.Select(flashcard => flashcard.ToDto()).ToList();
    }
}