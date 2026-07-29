using FluentValidation;

namespace CogniLink.Application.StudySessions.Commands.StartStudySession;

public sealed class StartStudySessionCommandValidator : AbstractValidator<StartStudySessionCommand>
{
    public StartStudySessionCommandValidator()
    {
        RuleFor(x => x.DeckId).NotEmpty();
    }
}