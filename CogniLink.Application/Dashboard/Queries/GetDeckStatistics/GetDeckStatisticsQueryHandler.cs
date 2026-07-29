using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Models;
using CogniLink.Application.StudySessions.Queries.GetDueFlashcards;
using CogniLink.Domain.Entities;
using MediatR;

namespace CogniLink.Application.Dashboard.Queries.GetDeckStatistics;

public sealed class GetDeckStatisticsQueryHandler : IRequestHandler<GetDeckStatisticsQuery, DeckStatisticsDto>
{
    private const string NoSubarea = "Sem subárea";

    private readonly IDeckRepository _deckRepository;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly IUserFlashcardProgressRepository _progressRepository;
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly IStudyAnswerRepository _studyAnswerRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;

    public GetDeckStatisticsQueryHandler(
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

    public async Task<DeckStatisticsDto> Handle(GetDeckStatisticsQuery request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUserService.UserId ?? throw new AuthenticationFailedException();

        var deck = await _deckRepository.GetByIdAsync(request.DeckId, cancellationToken);
        if (deck is null || !deck.OwnerId.Equals(ownerId, StringComparison.Ordinal))
        {
            throw new NotFoundException("Baralho não encontrado.");
        }

        var flashcards = await _flashcardRepository.ListByDeckIdAsync(deck.Id, cancellationToken);
        var flashcardsById = flashcards.ToDictionary(flashcard => flashcard.Id, StringComparer.Ordinal);

        var progressByFlashcard = (await _progressRepository.ListByOwnerAsync(ownerId, cancellationToken))
            .ToDictionary(progress => progress.FlashcardId, StringComparer.Ordinal);

        var masteredFlashcards = flashcards.Count(flashcard =>
            progressByFlashcard.TryGetValue(flashcard.Id, out var progress) && progress.IsMastered());

        var deckMasteryPercentage = flashcards.Count == 0
            ? 0d
            : (double)masteredFlashcards / flashcards.Count * 100;

        var dueFlashcards = await _mediator.Send(new GetDueFlashcardsQuery(), cancellationToken);
        var dueFlashcardsCount = dueFlashcards.Count(card => card.DeckId.Equals(request.DeckId, StringComparison.Ordinal));

        var sessions = (await _studySessionRepository.ListByOwnerAsync(ownerId, cancellationToken))
            .Where(session => session.DeckId.Equals(request.DeckId, StringComparison.Ordinal));

        var answers = new List<StudyAnswer>();
        foreach (var session in sessions)
        {
            answers.AddRange(await _studyAnswerRepository.ListBySessionIdAsync(session.Id, cancellationToken));
        }

        var answersWithFlashcard = answers
            .Where(answer => flashcardsById.ContainsKey(answer.FlashcardId))
            .Select(answer => (Answer: answer, Flashcard: flashcardsById[answer.FlashcardId]))
            .ToList();

        var performanceByDifficulty = flashcards
            .Select(flashcard => flashcard.Difficulty.ToString())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(key => key, StringComparer.Ordinal)
            .Select(key => BuildPerformanceGroup(key, answersWithFlashcard.Where(item => item.Flashcard.Difficulty.ToString() == key)))
            .ToList();

        var performanceBySubarea = flashcards
            .Select(flashcard => flashcard.Subarea ?? NoSubarea)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(key => key, StringComparer.Ordinal)
            .Select(key => BuildPerformanceGroup(key, answersWithFlashcard.Where(item => (item.Flashcard.Subarea ?? NoSubarea) == key)))
            .ToList();

        var performanceByType = flashcards
            .Select(flashcard => flashcard.Type.ToString())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(key => key, StringComparer.Ordinal)
            .Select(key => BuildPerformanceGroup(key, answersWithFlashcard.Where(item => item.Flashcard.Type.ToString() == key)))
            .ToList();

        return new DeckStatisticsDto(
            deck.Id,
            flashcards.Count,
            dueFlashcardsCount,
            deckMasteryPercentage,
            performanceByDifficulty,
            performanceBySubarea,
            performanceByType);
    }

    private static PerformanceGroupDto BuildPerformanceGroup(string groupKey, IEnumerable<(StudyAnswer Answer, Flashcard Flashcard)> items)
    {
        var group = items.ToList();
        var totalAnswers = group.Count;
        var correctAnswers = group.Count(item => item.Answer.IsCorrect);
        var accuracyRate = totalAnswers == 0 ? 0d : (double)correctAnswers / totalAnswers * 100;

        return new PerformanceGroupDto(groupKey, totalAnswers, correctAnswers, accuracyRate);
    }
}
