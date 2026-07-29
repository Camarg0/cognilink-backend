using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.StudySessions.Commands.CompleteStudySession;

public sealed record CompleteStudySessionCommand(string SessionId) : IRequest<StudySessionSummaryDto>;