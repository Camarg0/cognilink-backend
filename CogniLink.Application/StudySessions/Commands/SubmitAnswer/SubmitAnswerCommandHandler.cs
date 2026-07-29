using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Models;
using CogniLink.Domain.Entities;
using MediatR;

namespace CogniLink.Application.StudySessions.Commands.SubmitAnswer;

public sealed class SubmitAnswerCommandHandler : IRequestHandler<SubmitAnswerCommand, StudyAnswerResultDto>
{
    private const double DefaultEaseFactor = 2.5;

    private readonly IStudySessionRepository _studySessionRepository;
    private readonly IStudyAnswerRepository _studyAnswerRepository;
    private readonly IUserFlashcardProgressRepository _progressRepository;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly ICurrentUserService _currentUserService;

    public SubmitAnswerCommandHandler(
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

    public async Task<StudyAnswerResultDto> Handle(SubmitAnswerCommand request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUserService.UserId ?? throw new AuthenticationFailedException();

        var existingAnswer = await _studyAnswerRepository.GetByAttemptIdAsync(request.AttemptId, cancellationToken);
        if (existingAnswer is not null)
        {
            if (!string.Equals(existingAnswer.SessionId, request.SessionId, StringComparison.Ordinal))
            {
                throw new NotFoundException("Tentativa nao encontrada.");
            }

            var existingSession = await _studySessionRepository.GetByIdAndOwnerAsync(existingAnswer.SessionId, ownerId, cancellationToken);
            if (existingSession is null)
            {
                throw new NotFoundException("Tentativa nao encontrada.");
            }

            var existingProgress = await _progressRepository.GetByOwnerAndFlashcardAsync(ownerId, existingAnswer.FlashcardId, cancellationToken);
            return BuildResult(existingAnswer, existingProgress, wasIdempotentReplay: true);
        }

        var session = await _studySessionRepository.GetByIdAndOwnerAsync(request.SessionId, ownerId, cancellationToken);
        if (session is null)
        {
            throw new NotFoundException("Sessao nao encontrada.");
        }

        try
        {
            session.EnsureCanAcceptAnswers();
        }
        catch (InvalidOperationException)
        {
            throw new ConflictException("Sessao encerrada nao aceita novas respostas.");
        }

        var flashcards = await _flashcardRepository.ListByDeckIdAsync(session.DeckId, cancellationToken);
        var targetFlashcard = flashcards.FirstOrDefault(flashcard =>
            StudySessions.AttemptIdFactory.Create(session.Id, flashcard.Id) == request.AttemptId);

        if (targetFlashcard is null)
        {
            throw new NotFoundException("Tentativa nao encontrada.");
        }

        var progress = await _progressRepository.GetByOwnerAndFlashcardAsync(ownerId, targetFlashcard.Id, cancellationToken)
            ?? UserFlashcardProgress.Create(Guid.NewGuid().ToString("N"), targetFlashcard.Id, ownerId);

        progress.ApplyReview(request.IsCorrect, request.HintsViewed);

        var answer = StudyAnswer.Create(
            Guid.NewGuid().ToString("N"),
            session.Id,
            targetFlashcard.Id,
            request.AttemptId,
            request.IsCorrect,
            request.TimeToAnswerSeconds,
            request.HintsViewed);

        await _studyAnswerRepository.AddAsync(answer, cancellationToken);
        await _progressRepository.UpsertAsync(progress, cancellationToken);

        return BuildResult(answer, progress, wasIdempotentReplay: false);
    }

    private static StudyAnswerResultDto BuildResult(
        StudyAnswer answer,
        UserFlashcardProgress? progress,
        bool wasIdempotentReplay)
    {
        return new StudyAnswerResultDto(
            answer.SessionId,
            answer.FlashcardId,
            answer.AttemptId,
            wasIdempotentReplay,
            answer.IsCorrect,
            answer.AnsweredAt,
            progress?.EaseFactor ?? DefaultEaseFactor,
            progress?.IntervalDays ?? 0,
            progress?.NextReviewDate,
            progress?.Repetitions ?? 0,
            progress?.Lapses ?? 0,
            progress?.LastReviewedAt);
    }
}