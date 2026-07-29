using FluentValidation;

namespace CogniLink.Application.Flashcards.Queries.GetFlashcardById;

public sealed class GetFlashcardByIdQueryValidator : AbstractValidator<GetFlashcardByIdQuery>
{
    public GetFlashcardByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}