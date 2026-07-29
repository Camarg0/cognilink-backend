using FluentValidation;

namespace CogniLink.Application.StudySessions.Commands.SubmitAnswer;

public sealed class SubmitAnswerCommandValidator : AbstractValidator<SubmitAnswerCommand>
{
    public SubmitAnswerCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.AttemptId).NotEmpty();
        RuleFor(x => x.TimeToAnswerSeconds).GreaterThanOrEqualTo(0);
        RuleFor(x => x.HintsViewed).GreaterThanOrEqualTo(0);
    }
}