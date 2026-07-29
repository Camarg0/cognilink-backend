using CogniLink.Application.Common.Interfaces;
using CogniLink.Domain.Entities;
using CogniLink.Domain.Enums;
using Google.Cloud.Firestore;

namespace CogniLink.Infrastructure.Persistence.Firestore;

public sealed class FirestoreStudySessionRepository : IStudySessionRepository
{
    private const string CollectionName = "studySessions";
    private readonly FirestoreDb _firestoreDb;

    public FirestoreStudySessionRepository(FirestoreDb firestoreDb) => _firestoreDb = firestoreDb;

    public Task AddAsync(StudySession session, CancellationToken cancellationToken) =>
        _firestoreDb.Collection(CollectionName).Document(session.Id)
            .SetAsync(ToDocument(session), cancellationToken: cancellationToken);

    public async Task<StudySession?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var snapshot = await _firestoreDb.Collection(CollectionName).Document(id).GetSnapshotAsync(cancellationToken);
        return snapshot.Exists ? ToDomain(snapshot) : null;
    }

    public async Task<StudySession?> GetByIdAndOwnerAsync(string id, string ownerId, CancellationToken cancellationToken)
    {
        var snapshot = await _firestoreDb.Collection(CollectionName).Document(id).GetSnapshotAsync(cancellationToken);
        if (!snapshot.Exists)
        {
            return null;
        }

        var session = ToDomain(snapshot);
        return string.Equals(session.OwnerId, ownerId, StringComparison.Ordinal) ? session : null;
    }

    public async Task<IReadOnlyList<StudySession>> ListCompletedByOwnerAsync(string ownerId, CancellationToken cancellationToken)
    {
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("ownerId", ownerId)
            .WhereEqualTo("status", StudySessionStatus.Completed.ToString());

        var snapshot = await query.GetSnapshotAsync(cancellationToken);
        return snapshot.Documents.Select(ToDomain).ToList();
    }

    public Task UpdateAsync(StudySession session, CancellationToken cancellationToken) =>
        _firestoreDb.Collection(CollectionName).Document(session.Id)
            .SetAsync(ToDocument(session), cancellationToken: cancellationToken);

    private static StudySession ToDomain(DocumentSnapshot snapshot)
    {
        return StudySession.Reconstitute(
            snapshot.Id,
            snapshot.GetValue<string>("deckId"),
            snapshot.GetValue<string>("ownerId"),
            Enum.Parse<StudySessionStatus>(snapshot.GetValue<string>("status")),
            snapshot.GetValue<Timestamp>("startedAt").ToDateTime(),
            snapshot.ContainsField("completedAt") ? snapshot.GetValue<Timestamp?>("completedAt")?.ToDateTime() : null);
    }

    private static Dictionary<string, object?> ToDocument(StudySession session) => new()
    {
        ["deckId"] = session.DeckId,
        ["ownerId"] = session.OwnerId,
        ["status"] = session.Status.ToString(),
        ["startedAt"] = Timestamp.FromDateTime(DateTime.SpecifyKind(session.StartedAt, DateTimeKind.Utc)),
        ["completedAt"] = session.CompletedAt is null
            ? null
            : Timestamp.FromDateTime(DateTime.SpecifyKind(session.CompletedAt.Value, DateTimeKind.Utc))
    };
}
