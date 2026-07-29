using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.StudySessions.Queries.GetDueFlashcards;

public sealed class GetDueFlashcardsQueryHandler : IRequestHandler<GetDueFlashcardsQuery, IReadOnlyList<DueFlashcardDto>>
{
    private const double DefaultEaseFactor = 2.5;

    private readonly IDeckRepository _deckRepository;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly IUserFlashcardProgressRepository _progressRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetDueFlashcardsQueryHandler(
        IDeckRepository deckRepository,
        IFlashcardRepository flashcardRepository,
        IUserFlashcardProgressRepository progressRepository,
        ICurrentUserService currentUserService)
    {
        _deckRepository = deckRepository;
        _flashcardRepository = flashcardRepository;
        _progressRepository = progressRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<DueFlashcardDto>> Handle(GetDueFlashcardsQuery request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUserService.UserId ?? throw new AuthenticationFailedException();
        var utcNow = DateTime.UtcNow;

        var decks = await _deckRepository.GetByOwnerAsync(ownerId, cancellationToken);
        if (decks.Count == 0)
        {
            return [];
        }

        var progressByFlashcard = (await _progressRepository.ListByOwnerAsync(ownerId, cancellationToken))
            .ToDictionary(progress => progress.FlashcardId, StringComparer.Ordinal);

        var dueCards = new List<DueFlashcardDto>();

        foreach (var deck in decks)
        {
            var flashcards = await _flashcardRepository.ListByDeckIdAsync(deck.Id, cancellationToken);

            foreach (var flashcard in flashcards)
            {
                if (!progressByFlashcard.TryGetValue(flashcard.Id, out var progress))
                {
                    dueCards.Add(new DueFlashcardDto(
                        flashcard.Id,
                        deck.Id,
                        null,
                        DefaultEaseFactor,
                        0,
                        0,
                        0));
                    continue;
                }

                if (progress.NextReviewDate is null || progress.NextReviewDate <= utcNow)
                {
                    dueCards.Add(new DueFlashcardDto(
                        flashcard.Id,
                        deck.Id,
                        progress.NextReviewDate,
                        progress.EaseFactor,
                        progress.IntervalDays,
                        progress.Repetitions,
                        progress.Lapses));
                }
            }
        }

        return dueCards
            .OrderBy(card => card.NextReviewDate.HasValue ? 1 : 0)
            .ThenBy(card => card.NextReviewDate)
            .ThenBy(card => card.FlashcardId, StringComparer.Ordinal)
            .ToList();
    }
}