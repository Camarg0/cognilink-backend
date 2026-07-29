using FluentValidation;

namespace CogniLink.Application.Dashboard.Queries.GetStudyHistory;

public sealed class GetStudyHistoryQueryValidator : AbstractValidator<GetStudyHistoryQuery>
{
    private const int MaxRangeDays = 365;

    public GetStudyHistoryQueryValidator()
    {
        RuleFor(x => x.From).NotNull();
        RuleFor(x => x.To).NotNull();

        RuleFor(x => x)
            .Must(x => x.To!.Value >= x.From!.Value)
            .WithMessage("'To' must be greater than or equal to 'From'.")
            .When(x => x.From.HasValue && x.To.HasValue);

        RuleFor(x => x)
            .Must(x => x.To!.Value.DayNumber - x.From!.Value.DayNumber <= MaxRangeDays)
            .WithMessage($"The period between 'From' and 'To' must not exceed {MaxRangeDays} days.")
            .When(x => x.From.HasValue && x.To.HasValue);
    }
}
