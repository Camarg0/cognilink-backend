using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.StudySessions.Commands.CancelStudySession;

public sealed record CancelStudySessionCommand(string SessionId) : IRequest<StudySessionDto>;