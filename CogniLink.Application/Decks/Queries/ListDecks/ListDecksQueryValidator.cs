using FluentValidation;

namespace CogniLink.Application.Decks.Queries.ListDecks;

public sealed class ListDecksQueryValidator : AbstractValidator<ListDecksQuery>
{
    public ListDecksQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
