using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Models;
using CogniLink.Domain.Entities;
using CogniLink.Domain.Enums;
using MediatR;

namespace CogniLink.Application.StudySessions.Commands.CancelStudySession;

public sealed class CancelStudySessionCommandHandler : IRequestHandler<CancelStudySessionCommand, StudySessionDto>
{
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly ICurrentUserService _currentUserService;

    public CancelStudySessionCommandHandler(
        IStudySessionRepository studySessionRepository,
        ICurrentUserService currentUserService)
    {
        _studySessionRepository = studySessionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<StudySessionDto> Handle(CancelStudySessionCommand request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUserService.UserId ?? throw new AuthenticationFailedException();

        var session = await _studySessionRepository.GetByIdAndOwnerAsync(request.SessionId, ownerId, cancellationToken);
        if (session is null)
        {
            throw new NotFoundException("Sessao nao encontrada.");
        }

        // Cancelamento repetido e idempotente: devolve o estado ja registrado sem reprocessar.
        // Sessao concluida, porem, nao pode ser cancelada.
        if (session.Status == StudySessionStatus.Cancelled)
        {
            return ToDto(session);
        }

        if (!session.Cancel())
        {
            throw new ConflictException("Sessao ja concluida nao pode ser cancelada.");
        }

        await _studySessionRepository.UpdateAsync(session, cancellationToken);

        return ToDto(session);
    }

    private static StudySessionDto ToDto(StudySession session)
    {
        return new StudySessionDto(
            session.Id,
            session.DeckId,
            session.Status,
            session.StartedAt,
            session.CompletedAt);
    }
}