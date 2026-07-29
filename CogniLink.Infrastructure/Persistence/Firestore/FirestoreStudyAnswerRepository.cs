using CogniLink.Application.Common.Interfaces;
using CogniLink.Domain.Entities;
using Google.Cloud.Firestore;

namespace CogniLink.Infrastructure.Persistence.Firestore;

public sealed class FirestoreStudyAnswerRepository : IStudyAnswerRepository
{
    private const string CollectionName = "studyAnswers";
    private readonly FirestoreDb _firestoreDb;

    public FirestoreStudyAnswerRepository(FirestoreDb firestoreDb) => _firestoreDb = firestoreDb;

    public async Task<StudyAnswer?> GetByAttemptIdAsync(Guid attemptId, CancellationToken cancellationToken)
    {
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("attemptId", attemptId.ToString("D"))
            .Limit(1);

        var snapshot = await query.GetSnapshotAsync(cancellationToken);
        return snapshot.Count > 0 ? ToDomain(snapshot[0]) : null;
    }

    public async Task<IReadOnlyList<StudyAnswer>> ListBySessionIdAsync(string sessionId, CancellationToken cancellationToken)
    {
        var query = _firestoreDb.Collection(CollectionName)
            .WhereEqualTo("sessionId", sessionId);

        var snapshot = await query.GetSnapshotAsync(cancellationToken);
        return snapshot.Documents.Select(ToDomain).ToList();
    }

    public Task AddAsync(StudyAnswer answer, CancellationToken cancellationToken) =>
        _firestoreDb.Collection(CollectionName).Document(answer.Id)
            .SetAsync(ToDocument(answer), cancellationToken: cancellationToken);

    private static StudyAnswer ToDomain(DocumentSnapshot snapshot)
    {
        return StudyAnswer.Reconstitute(
            snapshot.Id,
            snapshot.GetValue<string>("sessionId"),
            snapshot.GetValue<string>("flashcardId"),
            Guid.Parse(snapshot.GetValue<string>("attemptId")),
            snapshot.GetValue<bool>("isCorrect"),
            snapshot.GetValue<int>("timeToAnswerSeconds"),
            snapshot.GetValue<int>("hintsViewed"),
            snapshot.GetValue<Timestamp>("answeredAt").ToDateTime());
    }

    private static Dictionary<string, object?> ToDocument(StudyAnswer answer) => new()
    {
        ["sessionId"] = answer.SessionId,
        ["flashcardId"] = answer.FlashcardId,
        ["attemptId"] = answer.AttemptId.ToString("D"),
        ["isCorrect"] = answer.IsCorrect,
        ["timeToAnswerSeconds"] = answer.TimeToAnswerSeconds,
        ["hintsViewed"] = answer.HintsViewed,
        ["answeredAt"] = Timestamp.FromDateTime(DateTime.SpecifyKind(answer.AnsweredAt, DateTimeKind.Utc))
    };
}
