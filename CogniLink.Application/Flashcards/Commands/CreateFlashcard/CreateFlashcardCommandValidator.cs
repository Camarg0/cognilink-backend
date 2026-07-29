using CogniLink.Domain.Enums;
using FluentValidation;
using System.Text.RegularExpressions;

namespace CogniLink.Application.Flashcards.Commands.CreateFlashcard;

public sealed class CreateFlashcardCommandValidator : AbstractValidator<CreateFlashcardCommand>
{
    public CreateFlashcardCommandValidator()
    {
        RuleFor(x => x.DeckId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Difficulty).IsInEnum();
        RuleFor(x => x.Subarea).MaximumLength(50);

        RuleFor(x => x.Hints)
            .Must(hints => hints is null || hints.Count <= 3)
            .WithMessage("A lista de dicas deve conter no maximo 3 itens.");
        RuleForEach(x => x.Hints).NotEmpty();

        When(x => x.Type == FlashcardType.FrenteVerso, () =>
        {
            RuleFor(x => x.Question).NotEmpty();
            RuleFor(x => x.Answer).NotEmpty();
        });

        When(x => x.Type == FlashcardType.Cloze, () =>
        {
            RuleFor(x => x.ClozeText)
                .NotEmpty()
                .Must(HasSequentialClozePlaceholders)
                .WithMessage("ClozeText deve conter lacunas sequenciais no formato {{c1::termo}}.");
        });

        When(x => x.Type == FlashcardType.DigiteResposta, () =>
        {
            RuleFor(x => x.ValidAnswers)
                .NotNull()
                .Must(validAnswers => validAnswers is { Count: > 0 })
                .WithMessage("DigiteResposta exige ao menos uma resposta valida.");
            RuleForEach(x => x.ValidAnswers).NotEmpty();
        });

        When(x => x.Type == FlashcardType.MultiplaEscolha, () =>
        {
            RuleFor(x => x.Alternatives)
                .NotNull()
                .Must(alternatives => alternatives is { Count: >= 2 and <= 5 })
                .WithMessage("MultiplaEscolha exige entre 2 e 5 alternativas.");
            RuleForEach(x => x.Alternatives!).ChildRules(alternative =>
            {
                alternative.RuleFor(item => item.Text).NotEmpty();
            });
            RuleFor(x => x.Alternatives)
                .Must(alternatives => alternatives is not null && alternatives.Count(item => item.IsCorrect) == 1)
                .WithMessage("MultiplaEscolha exige exatamente uma alternativa correta.");
        });
    }

    private static bool HasSequentialClozePlaceholders(string? clozeText)
    {
        if (string.IsNullOrWhiteSpace(clozeText))
        {
            return false;
        }

        var matches = Regex.Matches(clozeText, @"\{\{c(\d+)::([^{}]+)\}\}");
        if (matches.Count == 0)
        {
            return false;
        }

        var indices = matches
            .Cast<Match>()
            .Select(match => int.Parse(match.Groups[1].Value))
            .OrderBy(index => index)
            .ToArray();

        if (indices[0] != 1)
        {
            return false;
        }

        for (var index = 1; index < indices.Length; index++)
        {
            if (indices[index] != indices[index - 1] + 1)
            {
                return false;
            }
        }

        return true;
    }
}