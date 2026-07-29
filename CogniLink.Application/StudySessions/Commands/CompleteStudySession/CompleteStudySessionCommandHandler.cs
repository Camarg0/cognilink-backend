using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.StudySessions.Commands.CompleteStudySession;

public sealed class CompleteStudySessionCommandHandler : IRequestHandler<CompleteStudySessionCommand, StudySessionSummaryDto>
{
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly IStudyAnswerRepository _studyAnswerRepository;
    private readonly ICurrentUserService _currentUserService;

    public CompleteStudySessionCommandHandler(
        IStudySessionRepository studySessionRepository,
        IStudyAnswerRepository studyAnswerRepository,
        ICurrentUserService currentUserService)
    {
        _studySessionRepository = studySessionRepository;
        _studyAnswerRepository = studyAnswerRepository;
        _currentUserService = currentUserService;
    }

    public async Task<StudySessionSummaryDto> Handle(CompleteStudySessionCommand request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUserService.UserId ?? throw new AuthenticationFailedException();

        var session = await _studySessionRepository.GetByIdAndOwnerAsync(request.SessionId, ownerId, cancellationToken);
        if (session is null)
        {
            throw new NotFoundException("Sessao nao encontrada.");
        }

        if (!session.Complete())
        {
            throw new ConflictException("Sessao ja encerrada.");
        }

        await _studySessionRepository.UpdateAsync(session, cancellationToken);

        var answers = await _studyAnswerRepository.ListBySessionIdAsync(session.Id, cancellationToken);
        var cardsStudied = answers.Count;
        var correctAnswers = answers.Count(answer => answer.IsCorrect);
        var accuracyRate = cardsStudied == 0 ? 0d : correctAnswers / (double)cardsStudied;
        var durationSeconds = Math.Max(0, (int)Math.Round((session.CompletedAt!.Value - session.StartedAt).TotalSeconds));

        return new StudySessionSummaryDto(
            session.Id,
            session.Status,
            cardsStudied,
            correctAnswers,
            accuracyRate,
            durationSeconds,
            session.StartedAt,
            session.CompletedAt.Value);
    }
}