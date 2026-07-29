using FluentValidation;

namespace CogniLink.Application.Flashcards.Queries.ListFlashcardsByDeck;

public sealed class ListFlashcardsByDeckQueryValidator : AbstractValidator<ListFlashcardsByDeckQuery>
{
    public ListFlashcardsByDeckQueryValidator()
    {
        RuleFor(x => x.DeckId).NotEmpty();
    }
}