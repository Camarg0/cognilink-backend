using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Models;
using CogniLink.Application.StudySessions;
using MediatR;

namespace CogniLink.Application.StudySessions.Queries.GetNextCard;

public sealed class GetNextCardQueryHandler : IRequestHandler<GetNextCardQuery, NextStudyCardDto?>
{
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly IStudyAnswerRepository _studyAnswerRepository;
    private readonly IUserFlashcardProgressRepository _progressRepository;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetNextCardQueryHandler(
        IStudySessionRepository studySessionRepository,
        IStudyAnswerRepository studyAnswerRepository,
        IUserFlashcardProgressRepository progressRepository,
        IFlashcardRepository flashcardRepository,
        ICurrentUserService currentUserService)
    {
        _studySessionRepository = studySessionRepository;
        _studyAnswerRepository = studyAnswerRepository;
        _progressRepository = progressRepository;
        _flashcardRepository = flashcardRepository;
        _currentUserService = currentUserService;
    }

    public async Task<NextStudyCardDto?> Handle(GetNextCardQuery request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUserService.UserId ?? throw new AuthenticationFailedException();

        var session = await _studySessionRepository.GetByIdAndOwnerAsync(request.SessionId, ownerId, cancellationToken);
        if (session is null)
        {
            throw new NotFoundException("Sessao nao encontrada.");
        }

        if (session.IsClosed)
        {
            throw new ConflictException("Sessao ja encerrada.");
        }

        var answers = await _studyAnswerRepository.ListBySessionIdAsync(session.Id, cancellationToken);
        var answeredFlashcards = answers.Select(answer => answer.FlashcardId).ToHashSet(StringComparer.Ordinal);

        var deckFlashcards = await _flashcardRepository.ListByDeckIdAsync(session.DeckId, cancellationToken);
        var pendingFlashcards = deckFlashcards
            .Where(flashcard => !answeredFlashcards.Contains(flashcard.Id))
            .ToList();

        if (pendingFlashcards.Count == 0)
        {
            return null;
        }

        var progressByFlashcard = (await _progressRepository.ListByOwnerAsync(ownerId, cancellationToken))
            .ToDictionary(progress => progress.FlashcardId, StringComparer.Ordinal);

        var utcNow = DateTime.UtcNow;

        var neverReviewedCard = pendingFlashcards
            .Where(flashcard =>
            {
                return !progressByFlashcard.TryGetValue(flashcard.Id, out var progress)
                    || progress.NextReviewDate is null;
            })
            .OrderBy(flashcard => flashcard.CreatedAt)
            .ThenBy(flashcard => flashcard.Id, StringComparer.Ordinal)
            .FirstOrDefault();

        var selectedFlashcard = neverReviewedCard;
        var dueState = "NEVER_REVIEWED";

        if (selectedFlashcard is null)
        {
            selectedFlashcard = pendingFlashcards
                .Where(flashcard =>
                    progressByFlashcard.TryGetValue(flashcard.Id, out var progress)
                    && progress.NextReviewDate is not null
                    && progress.NextReviewDate <= utcNow)
                .OrderBy(flashcard => progressByFlashcard[flashcard.Id].NextReviewDate)
                .ThenBy(flashcard => flashcard.Id, StringComparer.Ordinal)
                .FirstOrDefault();

            dueState = "OVERDUE";
        }

        if (selectedFlashcard is null)
        {
            return null;
        }

        var alternatives = selectedFlashcard.Alternatives.Count == 0
            ? null
            : selectedFlashcard.Alternatives
                .Select(alternative => new FlashcardAlternativeDto(alternative.Text, alternative.IsCorrect))
                .ToList();

        var hints = selectedFlashcard.Hints.Count == 0 ? null : selectedFlashcard.Hints.ToList();

        return new NextStudyCardDto(
            session.Id,
            selectedFlashcard.Id,
            AttemptIdFactory.Create(session.Id, selectedFlashcard.Id),
            selectedFlashcard.Type,
            selectedFlashcard.Difficulty,
            selectedFlashcard.Question,
            selectedFlashcard.Answer,
            selectedFlashcard.ClozeText,
            alternatives,
            hints,
            dueState);
    }
}