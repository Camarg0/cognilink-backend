using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Models;
using CogniLink.Application.StudySessions.Queries.GetDueFlashcards;
using CogniLink.Application.StudySessions.Queries.GetUserStreak;
using MediatR;

namespace CogniLink.Application.Dashboard.Queries.GetDashboard;

public sealed class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardSummaryDto>
{
    private readonly IDeckRepository _deckRepository;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly IUserFlashcardProgressRepository _progressRepository;
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly IStudyAnswerRepository _studyAnswerRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;

    public GetDashboardQueryHandler(
        IDeckRepository deckRepository,
        IFlashcardRepository flashcardRepository,
        IUserFlashcardProgressRepository progressRepository,
        IStudySessionRepository studySessionRepository,
        IStudyAnswerRepository studyAnswerRepository,
        ICurrentUserService currentUserService,
        IMediator mediator)
    {
        _deckRepository = deckRepository;
        _flashcardRepository = flashcardRepository;
        _progressRepository = progressRepository;
        _studySessionRepository = studySessionRepository;
        _studyAnswerRepository = studyAnswerRepository;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUserService.UserId ?? throw new AuthenticationFailedException();

        var progressByFlashcard = (await _progressRepository.ListByOwnerAsync(ownerId, cancellationToken))
            .ToDictionary(progress => progress.FlashcardId, StringComparer.Ordinal);

        var decks = await _deckRepository.GetByOwnerAsync(ownerId, cancellationToken);

        var totalFlashcards = 0;
        var masteredFlashcards = 0;
        foreach (var deck in decks)
        {
            var flashcards = await _flashcardRepository.ListByDeckIdAsync(deck.Id, cancellationToken);
            totalFlashcards += flashcards.Count;
            foreach (var flashcard in flashcards)
            {
                if (progressByFlashcard.TryGetValue(flashcard.Id, out var progress) && progress.IsMastered())
                {
                    masteredFlashcards++;
                }
            }
        }

        var masteryPercentage = totalFlashcards == 0 ? 0d : (double)masteredFlashcards / totalFlashcards * 100;

        var sessions = await _studySessionRepository.ListByOwnerAsync(ownerId, cancellationToken);

        var totalStudyTimeSeconds = 0L;
        var totalAnswersCount = 0;
        var correctAnswersCount = 0;
        var answeredFlashcards = new HashSet<string>(StringComparer.Ordinal);

        foreach (var session in sessions)
        {
            var answers = await _studyAnswerRepository.ListBySessionIdAsync(session.Id, cancellationToken);
            foreach (var answer in answers)
            {
                totalStudyTimeSeconds += answer.TimeToAnswerSeconds;
                totalAnswersCount++;
                answeredFlashcards.Add(answer.FlashcardId);
                if (answer.IsCorrect)
                {
                    correctAnswersCount++;
                }
            }
        }

        var completedCardsCount = answeredFlashcards.Count;
        var retentionRate = totalAnswersCount == 0 ? 0d : (double)correctAnswersCount / totalAnswersCount * 100;

        var streak = await _mediator.Send(new GetUserStreakQuery(), cancellationToken);
        var dueFlashcards = await _mediator.Send(new GetDueFlashcardsQuery(), cancellationToken);

        return new DashboardSummaryDto(
            masteryPercentage,
            totalStudyTimeSeconds,
            completedCardsCount,
            retentionRate,
            streak.CurrentStreak,
            streak.LongestStreak,
            dueFlashcards.Count);
    }
}
