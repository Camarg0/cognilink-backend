namespace CogniLink.Domain.Entities;

public sealed class UserFlashcardProgress
{
    private const double DefaultEaseFactor = 2.5;
    private const double EaseFactorFloor = 1.3;
    private const double CorrectAnswerEaseBonus = 0.1;
    private const double WrongAnswerEasePenalty = 0.2;

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

    public void ApplyReview(bool isCorrect)
    {
        var reviewedAt = DateTime.UtcNow;

        if (!isCorrect)
        {
            Lapses++;
            Repetitions = 0;
            IntervalDays = 1;
            EaseFactor = Math.Max(EaseFactorFloor, EaseFactor - WrongAnswerEasePenalty);
            LastReviewedAt = reviewedAt;
            NextReviewDate = reviewedAt.AddDays(IntervalDays);
            return;
        }

        Repetitions++;
        EaseFactor = Math.Max(EaseFactorFloor, EaseFactor + CorrectAnswerEaseBonus);

        IntervalDays = Repetitions switch
        {
            1 => 1,
            2 => 6,
            _ => Math.Max(1, (int)Math.Round(IntervalDays * EaseFactor, MidpointRounding.AwayFromZero))
        };

        LastReviewedAt = reviewedAt;
        NextReviewDate = reviewedAt.AddDays(IntervalDays);
    }
}