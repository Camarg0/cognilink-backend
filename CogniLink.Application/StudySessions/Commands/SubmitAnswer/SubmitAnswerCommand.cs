using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.StudySessions.Commands.SubmitAnswer;

public sealed record SubmitAnswerCommand(
    string SessionId,
    Guid AttemptId,
    bool IsCorrect,
    int TimeToAnswerSeconds,
    int HintsViewed) : IRequest<StudyAnswerResultDto>;