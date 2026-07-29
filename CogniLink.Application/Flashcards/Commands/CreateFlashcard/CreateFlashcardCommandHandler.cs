using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Mappings;
using CogniLink.Application.Common.Models;
using CogniLink.Domain.Entities;
using CogniLink.Domain.ValueObjects;
using MediatR;

namespace CogniLink.Application.Flashcards.Commands.CreateFlashcard;

public sealed class CreateFlashcardCommandHandler : IRequestHandler<CreateFlashcardCommand, FlashcardDto>
{
    private readonly IDeckRepository _deckRepository;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateFlashcardCommandHandler(
        IDeckRepository deckRepository,
        IFlashcardRepository flashcardRepository,
        ICurrentUserService currentUserService)
    {
        _deckRepository = deckRepository;
        _flashcardRepository = flashcardRepository;
        _currentUserService = currentUserService;
    }

    public async Task<FlashcardDto> Handle(CreateFlashcardCommand request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUserService.UserId ?? throw new AuthenticationFailedException();

        var deck = await _deckRepository.GetByIdAsync(request.DeckId, cancellationToken);
        if (deck is null || deck.OwnerId != ownerId)
        {
            throw new NotFoundException("Baralho nao encontrado.");
        }

        var flashcard = Flashcard.Create(
            Guid.NewGuid().ToString("N"),
            request.DeckId,
            request.Type,
            request.Difficulty,
            request.Subarea,
            request.Hints,
            request.Question,
            request.Answer,
            request.ClozeText,
            request.ValidAnswers,
            request.Alternatives?.Select(alternative => Alternative.Create(alternative.Text, alternative.IsCorrect)).ToList());

        await _flashcardRepository.AddAsync(flashcard, cancellationToken);

        return flashcard.ToDto();
    }
}