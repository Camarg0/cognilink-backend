using CogniLink.Application.Common.Interfaces;
using CogniLink.Domain.Entities;
using Google.Cloud.Firestore;

namespace CogniLink.Infrastructure.Persistence.Firestore;

public sealed class FirestoreUserFlashcardProgressRepository : IUserFlashcardProgressRepository
{
    private const string CollectionName = "userFlashcardProgress";
    private readonly FirestoreDb _firestoreDb;

    public FirestoreUserFlashcardProgressRepository(FirestoreDb firestoreDb) => _firestoreDb = firestoreDb;

    public async Task<UserFlashcardProgress?> GetByOwnerAndFlashcardAsync(string ownerId, string flashcardId, CancellationToken cancellationToken)
    {
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("ownerId", ownerId)
            .WhereEqualTo("flashcardId", flashcardId)
            .Limit(1);

        var snapshot = await query.GetSnapshotAsync(cancellationToken);
        return snapshot.Count > 0 ? ToDomain(snapshot[0]) : null;
    }

    public async Task<IReadOnlyList<UserFlashcardProgress>> ListByOwnerAsync(string ownerId, CancellationToken cancellationToken)
    {
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("ownerId", ownerId);

        var snapshot = await query.GetSnapshotAsync(cancellationToken);
        return snapshot.Documents.Select(ToDomain).ToList();
    }

    public Task UpsertAsync(UserFlashcardProgress progress, CancellationToken cancellationToken) =>
        _firestoreDb.Collection(CollectionName).Document(progress.Id)
            .SetAsync(ToDocument(progress), cancellationToken: cancellationToken);

    private static UserFlashcardProgress ToDomain(DocumentSnapshot snapshot)
    {
        return UserFlashcardProgress.Reconstitute(
            snapshot.Id,
            snapshot.GetValue<string>("flashcardId"),
            snapshot.GetValue<string>("ownerId"),
            snapshot.GetValue<double>("easeFactor"),
            snapshot.GetValue<int>("intervalDays"),
            snapshot.ContainsField("nextReviewDate") ? snapshot.GetValue<Timestamp?>("nextReviewDate")?.ToDateTime() : null,
            snapshot.GetValue<int>("repetitions"),
            snapshot.GetValue<int>("lapses"),
            snapshot.ContainsField("lastReviewedAt") ? snapshot.GetValue<Timestamp?>("lastReviewedAt")?.ToDateTime() : null);
    }

    private static Dictionary<string, object?> ToDocument(UserFlashcardProgress progress) => new()
    {
        ["flashcardId"] = progress.FlashcardId,
        ["ownerId"] = progress.OwnerId,
        ["easeFactor"] = progress.EaseFactor,
        ["intervalDays"] = progress.IntervalDays,
        ["nextReviewDate"] = progress.NextReviewDate is null
            ? null
            : Timestamp.FromDateTime(DateTime.SpecifyKind(progress.NextReviewDate.Value, DateTimeKind.Utc)),
        ["repetitions"] = progress.Repetitions,
        ["lapses"] = progress.Lapses,
        ["lastReviewedAt"] = progress.LastReviewedAt is null
            ? null
            : Timestamp.FromDateTime(DateTime.SpecifyKind(progress.LastReviewedAt.Value, DateTimeKind.Utc))
    };
}
