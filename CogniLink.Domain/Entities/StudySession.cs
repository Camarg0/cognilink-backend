using CogniLink.Domain.Enums;

namespace CogniLink.Domain.Entities;

public sealed class StudySession
{
    public string Id { get; private set; } = string.Empty;
    public string DeckId { get; private set; } = string.Empty;
    public string OwnerId { get; private set; } = string.Empty;
    public StudySessionStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private StudySession()
    {
    }

    public bool IsClosed => Status is StudySessionStatus.Completed or StudySessionStatus.Cancelled;

    public static StudySession Create(string id, string deckId, string ownerId)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(deckId)) throw new ArgumentException("DeckId is required.", nameof(deckId));
        if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("OwnerId is required.", nameof(ownerId));

        return new StudySession
        {
            Id = id,
            DeckId = deckId,
            OwnerId = ownerId,
            Status = StudySessionStatus.InProgress,
            StartedAt = DateTime.UtcNow
        };
    }

    public static StudySession Reconstitute(
        string id,
        string deckId,
        string ownerId,
        StudySessionStatus status,
        DateTime startedAt,
        DateTime? completedAt)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(deckId)) throw new ArgumentException("DeckId is required.", nameof(deckId));
        if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("OwnerId is required.", nameof(ownerId));
        if (!Enum.IsDefined(status)) throw new ArgumentOutOfRangeException(nameof(status), status, "Invalid session status.");
        if (status is StudySessionStatus.InProgress && completedAt is not null)
        {
            throw new ArgumentException("In-progress sessions cannot have CompletedAt.", nameof(completedAt));
        }

        if (status is StudySessionStatus.Completed or StudySessionStatus.Cancelled && completedAt is null)
        {
            throw new ArgumentException("Closed sessions must have CompletedAt.", nameof(completedAt));
        }

        return new StudySession
        {
            Id = id,
            DeckId = deckId,
            OwnerId = ownerId,
            Status = status,
            StartedAt = startedAt,
            CompletedAt = completedAt
        };
    }

    public bool Complete()
    {
        if (Status == StudySessionStatus.Completed)
        {
            return false;
        }

        if (Status == StudySessionStatus.Cancelled)
        {
            return false;
        }

        Status = StudySessionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        return true;
    }

    public bool Cancel()
    {
        if (Status == StudySessionStatus.Cancelled)
        {
            return false;
        }

        if (Status == StudySessionStatus.Completed)
        {
            return false;
        }

        Status = StudySessionStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
        return true;
    }

    public void EnsureCanAcceptAnswers()
    {
        if (Status != StudySessionStatus.InProgress)
        {
            throw new InvalidOperationException("Session is closed and cannot accept answers.");
        }
    }
}