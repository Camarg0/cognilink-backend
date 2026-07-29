using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.StudySessions.Commands.StartStudySession;

public sealed record StartStudySessionCommand(string DeckId) : IRequest<StudySessionDto>;