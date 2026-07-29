using FluentValidation;

namespace CogniLink.Application.Decks.Commands.UpdateDeck;

public sealed class UpdateDeckCommandValidator : AbstractValidator<UpdateDeckCommand>
{
    public UpdateDeckCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Difficulty).IsInEnum();
        RuleFor(x => x.Categories)
            .Must(categories => categories is null || categories.Count <= 5)
            .WithMessage("A lista de categorias deve conter no máximo 5 itens.");
        RuleForEach(x => x.Categories).MaximumLength(30);
    }
}
