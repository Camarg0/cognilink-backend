using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Mappings;
using CogniLink.Application.Common.Models;
using CogniLink.Domain.ValueObjects;
using MediatR;

namespace CogniLink.Application.Flashcards.Commands.UpdateFlashcard;

public sealed class UpdateFlashcardCommandHandler : IRequestHandler<UpdateFlashcardCommand, FlashcardDto>
{
    private readonly IDeckRepository _deckRepository;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateFlashcardCommandHandler(
        IDeckRepository deckRepository,
        IFlashcardRepository flashcardRepository,
        ICurrentUserService currentUserService)
    {
        _deckRepository = deckRepository;
        _flashcardRepository = flashcardRepository;
        _currentUserService = currentUserService;
    }

    public async Task<FlashcardDto> Handle(UpdateFlashcardCommand request, CancellationToken cancellationToken)
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

        flashcard.Update(
            request.Type,
            request.Difficulty,
            request.Subarea,
            request.Hints,
            request.Question,
            request.Answer,
            request.ClozeText,
            request.ValidAnswers,
            request.Alternatives?.Select(alternative => Alternative.Create(alternative.Text, alternative.IsCorrect)).ToList());

        await _flashcardRepository.UpdateAsync(flashcard, cancellationToken);

        return flashcard.ToDto();
    }
}