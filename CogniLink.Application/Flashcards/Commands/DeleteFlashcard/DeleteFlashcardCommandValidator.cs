using FluentValidation;

namespace CogniLink.Application.Flashcards.Commands.DeleteFlashcard;

public sealed class DeleteFlashcardCommandValidator : AbstractValidator<DeleteFlashcardCommand>
{
    public DeleteFlashcardCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}