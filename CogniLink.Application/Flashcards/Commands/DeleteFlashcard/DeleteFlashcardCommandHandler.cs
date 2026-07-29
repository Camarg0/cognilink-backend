using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using MediatR;

namespace CogniLink.Application.Flashcards.Commands.DeleteFlashcard;

public sealed class DeleteFlashcardCommandHandler : IRequestHandler<DeleteFlashcardCommand>
{
    private readonly IDeckRepository _deckRepository;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteFlashcardCommandHandler(
        IDeckRepository deckRepository,
        IFlashcardRepository flashcardRepository,
        ICurrentUserService currentUserService)
    {
        _deckRepository = deckRepository;
        _flashcardRepository = flashcardRepository;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteFlashcardCommand request, CancellationToken cancellationToken)
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

        await _flashcardRepository.DeleteAsync(request.Id, cancellationToken);
    }
}