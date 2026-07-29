using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Mappings;
using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.Flashcards.Queries.GetFlashcardById;

public sealed class GetFlashcardByIdQueryHandler : IRequestHandler<GetFlashcardByIdQuery, FlashcardDto>
{
    private readonly IDeckRepository _deckRepository;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetFlashcardByIdQueryHandler(
        IDeckRepository deckRepository,
        IFlashcardRepository flashcardRepository,
        ICurrentUserService currentUserService)
    {
        _deckRepository = deckRepository;
        _flashcardRepository = flashcardRepository;
        _currentUserService = currentUserService;
    }

    public async Task<FlashcardDto> Handle(GetFlashcardByIdQuery request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUserService.UserId ?? throw new AuthenticationFailedException();

        var flashcard = await _flashcardRepository.GetByIdAsync(request.Id, cancellationToken);
        if (flashcard is null)
        {
            throw new NotFoundException("Flashcard nao encontrado.");
        }

        var deck = await _deckRepository.GetByIdAsync(flashcard.DeckId, cancellationToken);
        if (deck is null || deck.OwnerId != ownerId)
        {
            throw new NotFoundException("Flashcard nao encontrado.");
        }

        return flashcard.ToDto();
    }
}