using CogniLink.Application.Common.Models;
using CogniLink.Domain.Entities;
using Mapster;

namespace CogniLink.Application.Common.Mappings;

public static class FlashcardMappings
{
    static FlashcardMappings()
    {
        TypeAdapterConfig<Flashcard, FlashcardDto>.NewConfig()
            .Map(
                destination => destination.ValidAnswers,
                source => source.ValidAnswers.Count == 0 ? null : source.ValidAnswers.ToList())
            .Map(
                destination => destination.Alternatives,
                source => source.Alternatives.Count == 0
                    ? null
                    : source.Alternatives
                        .Select(alternative => new FlashcardAlternativeDto(alternative.Text, alternative.IsCorrect))
                        .ToList());
    }

    public static FlashcardDto ToDto(this Flashcard flashcard)
    {
        _ = typeof(FlashcardMappings);
        return flashcard.Adapt<FlashcardDto>();
    }
}