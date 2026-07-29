namespace CogniLink.Domain.Entities;

public sealed class UserFlashcardProgress
{
    private const double DefaultEaseFactor = 2.5;
    private const double EaseFactorFloor = 1.3;

    // Qualidade SM-2 (0-5) derivada do acerto e da quantidade de dicas consultadas.
    private const int QualityWrongAnswer = 2;
    private const int QualityCorrectNoHints = 5;
    private const int QualityCorrectOneHint = 4;
    private const int QualityCorrectManyHints = 3;

    public string Id { get; private set; } = string.Empty;
    public string FlashcardId { get; private set; } = string.Empty;
    public string OwnerId { get; private set; } = string.Empty;
    public double EaseFactor { get; private set; }
    public int IntervalDays { get; private set; }
    public DateTime? NextReviewDate { get; private set; }
    public int Repetitions { get; private set; }
    public int Lapses { get; private set; }
    public DateTime? LastReviewedAt { get; private set; }

    private UserFlashcardProgress()
    {
    }

    public static UserFlashcardProgress Create(string id, string flashcardId, string ownerId)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(flashcardId)) throw new ArgumentException("FlashcardId is required.", nameof(flashcardId));
        if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("OwnerId is required.", nameof(ownerId));

        return new UserFlashcardProgress
        {
            Id = id,
            FlashcardId = flashcardId,
            OwnerId = ownerId,
            EaseFactor = DefaultEaseFactor,
            IntervalDays = 0,
            Repetitions = 0,
            Lapses = 0
        };
    }

    public static UserFlashcardProgress Reconstitute(
        string id,
        string flashcardId,
        string ownerId,
        double easeFactor,
        int intervalDays,
        DateTime? nextReviewDate,
        int repetitions,
        int lapses,
        DateTime? lastReviewedAt)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(flashcardId)) throw new ArgumentException("FlashcardId is required.", nameof(flashcardId));
        if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("OwnerId is required.", nameof(ownerId));
        if (easeFactor < EaseFactorFloor) throw new ArgumentOutOfRangeException(nameof(easeFactor), $"EaseFactor must be at least {EaseFactorFloor}.");
        if (intervalDays < 0) throw new ArgumentOutOfRangeException(nameof(intervalDays), "IntervalDays must be greater than or equal to zero.");
        if (repetitions < 0) throw new ArgumentOutOfRangeException(nameof(repetitions), "Repetitions must be greater than or equal to zero.");
        if (lapses < 0) throw new ArgumentOutOfRangeException(nameof(lapses), "Lapses must be greater than or equal to zero.");

        return new UserFlashcardProgress
        {
            Id = id,
            FlashcardId = flashcardId,
            OwnerId = ownerId,
            EaseFactor = easeFactor,
            IntervalDays = intervalDays,
            NextReviewDate = nextReviewDate,
            Repetitions = repetitions,
            Lapses = lapses,
            LastReviewedAt = lastReviewedAt
        };
    }

    /// <summary>
    /// Recalcula o agendamento SM-2 do card. A qualidade da resposta considera o acerto e
    /// quantas dicas foram consultadas: acertar sem dicas eleva o fator de facilidade,
    /// enquanto acertar apoiado em dicas o mantem ou reduz.
    /// </summary>
    public void ApplyReview(bool isCorrect, int hintsViewed)
    {
        if (hintsViewed < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(hintsViewed), "HintsViewed must be greater than or equal to zero.");
        }

        var reviewedAt = DateTime.UtcNow;
        var quality = ResolveQuality(isCorrect, hintsViewed);

        EaseFactor = ApplyEaseFactorFormula(EaseFactor, quality);

        if (!isCorrect)
        {
            Lapses++;
            Repetitions = 0;
            IntervalDays = 1;
            LastReviewedAt = reviewedAt;
            NextReviewDate = reviewedAt.AddDays(IntervalDays);
            return;
        }

        Repetitions++;

        IntervalDays = Repetitions switch
        {
            1 => 1,
            2 => 6,
            _ => Math.Max(1, (int)Math.Round(IntervalDays * EaseFactor, MidpointRounding.AwayFromZero))
        };

        LastReviewedAt = reviewedAt;
        NextReviewDate = reviewedAt.AddDays(IntervalDays);
    }

    private static int ResolveQuality(bool isCorrect, int hintsViewed)
    {
        if (!isCorrect)
        {
            return QualityWrongAnswer;
        }

        return hintsViewed switch
        {
            0 => QualityCorrectNoHints,
            1 => QualityCorrectOneHint,
            _ => QualityCorrectManyHints
        };
    }

    /// <summary>
    /// Formula classica de fator de facilidade do SM-2 (Wozniak):
    /// EF' = EF + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.02)), com piso de 1.3.
    /// </summary>
    private static double ApplyEaseFactorFormula(double easeFactor, int quality)
    {
        var qualityGap = 5 - quality;
        var adjustment = 0.1 - (qualityGap * (0.08 + (qualityGap * 0.02)));

        return Math.Max(EaseFactorFloor, easeFactor + adjustment);
    }

    public bool IsMastered() => Repetitions >= 2 && IntervalDays >= 21;
}