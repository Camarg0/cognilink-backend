namespace CogniLink.Domain.Entities;

public sealed class StudyAnswer
{
    public string Id { get; private set; } = string.Empty;
    public string SessionId { get; private set; } = string.Empty;
    public string FlashcardId { get; private set; } = string.Empty;
    public Guid AttemptId { get; private set; }
    public bool IsCorrect { get; private set; }
    public int TimeToAnswerSeconds { get; private set; }
    public int HintsViewed { get; private set; }
    public DateTime AnsweredAt { get; private set; }

    private StudyAnswer()
    {
    }

    public static StudyAnswer Create(
        string id,
        string sessionId,
        string flashcardId,
        Guid attemptId,
        bool isCorrect,
        int timeToAnswerSeconds,
        int hintsViewed)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentException("SessionId is required.", nameof(sessionId));
        if (string.IsNullOrWhiteSpace(flashcardId)) throw new ArgumentException("FlashcardId is required.", nameof(flashcardId));
        if (attemptId == Guid.Empty) throw new ArgumentException("AttemptId is required.", nameof(attemptId));
        if (timeToAnswerSeconds < 0) throw new ArgumentOutOfRangeException(nameof(timeToAnswerSeconds), "TimeToAnswerSeconds must be greater than or equal to zero.");
        if (hintsViewed < 0) throw new ArgumentOutOfRangeException(nameof(hintsViewed), "HintsViewed must be greater than or equal to zero.");

        return new StudyAnswer
        {
            Id = id,
            SessionId = sessionId,
            FlashcardId = flashcardId,
            AttemptId = attemptId,
            IsCorrect = isCorrect,
            TimeToAnswerSeconds = timeToAnswerSeconds,
            HintsViewed = hintsViewed,
            AnsweredAt = DateTime.UtcNow
        };
    }

    public static StudyAnswer Reconstitute(
        string id,
        string sessionId,
        string flashcardId,
        Guid attemptId,
        bool isCorrect,
        int timeToAnswerSeconds,
        int hintsViewed,
        DateTime answeredAt)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentException("SessionId is required.", nameof(sessionId));
        if (string.IsNullOrWhiteSpace(flashcardId)) throw new ArgumentException("FlashcardId is required.", nameof(flashcardId));
        if (attemptId == Guid.Empty) throw new ArgumentException("AttemptId is required.", nameof(attemptId));
        if (timeToAnswerSeconds < 0) throw new ArgumentOutOfRangeException(nameof(timeToAnswerSeconds), "TimeToAnswerSeconds must be greater than or equal to zero.");
        if (hintsViewed < 0) throw new ArgumentOutOfRangeException(nameof(hintsViewed), "HintsViewed must be greater than or equal to zero.");

        return new StudyAnswer
        {
            Id = id,
            SessionId = sessionId,
            FlashcardId = flashcardId,
            AttemptId = attemptId,
            IsCorrect = isCorrect,
            TimeToAnswerSeconds = timeToAnswerSeconds,
            HintsViewed = hintsViewed,
            AnsweredAt = answeredAt
        };
    }

    public void EnsureMatchesAttempt(Guid attemptId, string sessionId, string flashcardId)
    {
        if (attemptId == Guid.Empty)
        {
            throw new ArgumentException("AttemptId is required.", nameof(attemptId));
        }

        if (!AttemptId.Equals(attemptId) || !SessionId.Equals(sessionId, StringComparison.Ordinal) || !FlashcardId.Equals(flashcardId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Attempt metadata does not match the original study answer.");
        }
    }
}