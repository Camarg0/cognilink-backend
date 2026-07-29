using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Models;
using CogniLink.Domain.Entities;
using MediatR;

namespace CogniLink.Application.StudySessions.Commands.StartStudySession;

public sealed class StartStudySessionCommandHandler : IRequestHandler<StartStudySessionCommand, StudySessionDto>
{
    private readonly IDeckRepository _deckRepository;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly ICurrentUserService _currentUserService;

    public StartStudySessionCommandHandler(
        IDeckRepository deckRepository,
        IFlashcardRepository flashcardRepository,
        IStudySessionRepository studySessionRepository,
        ICurrentUserService currentUserService)
    {
        _deckRepository = deckRepository;
        _flashcardRepository = flashcardRepository;
        _studySessionRepository = studySessionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<StudySessionDto> Handle(StartStudySessionCommand request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUserService.UserId ?? throw new AuthenticationFailedException();

        var deck = await _deckRepository.GetByIdAsync(request.DeckId, cancellationToken);
        if (deck is null || deck.OwnerId != ownerId)
        {
            throw new NotFoundException("Baralho nao encontrado.");
        }

        var flashcards = await _flashcardRepository.ListByDeckIdAsync(deck.Id, cancellationToken);
        if (flashcards.Count == 0)
        {
            throw new ConflictException("Nao e possivel iniciar sessao em baralho sem flashcards.");
        }

        var session = StudySession.Create(Guid.NewGuid().ToString("N"), deck.Id, ownerId);
        await _studySessionRepository.AddAsync(session, cancellationToken);

        return new StudySessionDto(
            session.Id,
            session.DeckId,
            session.Status,
            session.StartedAt,
            session.CompletedAt);
    }
}