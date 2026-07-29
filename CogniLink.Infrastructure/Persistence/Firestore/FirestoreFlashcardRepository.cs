using CogniLink.Application.Common.Interfaces;
using CogniLink.Domain.Entities;
using CogniLink.Domain.Enums;
using CogniLink.Domain.ValueObjects;
using Google.Cloud.Firestore;

namespace CogniLink.Infrastructure.Persistence.Firestore;

public sealed class FirestoreFlashcardRepository : IFlashcardRepository
{
    private const string CollectionName = "flashcards";
    private readonly FirestoreDb _firestoreDb;

    public FirestoreFlashcardRepository(FirestoreDb firestoreDb) => _firestoreDb = firestoreDb;

    public Task AddAsync(Flashcard flashcard, CancellationToken cancellationToken) =>
        _firestoreDb.Collection(CollectionName).Document(flashcard.Id)
            .SetAsync(ToDocument(flashcard), cancellationToken: cancellationToken);

    public async Task<Flashcard?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var snapshot = await _firestoreDb.Collection(CollectionName).Document(id).GetSnapshotAsync(cancellationToken);
        return snapshot.Exists ? ToDomain(snapshot) : null;
    }

    public async Task<IReadOnlyList<Flashcard>> ListByDeckIdAsync(string deckId, CancellationToken cancellationToken)
    {
        var query = _firestoreDb.Collection(CollectionName).WhereEqualTo("deckId", deckId);
        var snapshot = await query.GetSnapshotAsync(cancellationToken);
        return snapshot.Documents.Select(ToDomain).ToList();
    }

    public Task UpdateAsync(Flashcard flashcard, CancellationToken cancellationToken) =>
        _firestoreDb.Collection(CollectionName).Document(flashcard.Id)
            .SetAsync(ToDocument(flashcard), cancellationToken: cancellationToken);

    public Task DeleteAsync(string id, CancellationToken cancellationToken) =>
        _firestoreDb.Collection(CollectionName).Document(id).DeleteAsync(cancellationToken: cancellationToken);

    private static Flashcard ToDomain(DocumentSnapshot snapshot)
    {
        var hints = snapshot.ContainsField("hints")
            ? snapshot.GetValue<List<string>>("hints")
            : [];

        var validAnswers = snapshot.ContainsField("validAnswers")
            ? snapshot.GetValue<List<string>>("validAnswers")
            : [];

        var alternatives = snapshot.ContainsField("alternatives")
            ? snapshot.GetValue<List<Dictionary<string, object>>>("alternatives")
                .Select(alternative => Alternative.Reconstitute(
                    Convert.ToString(alternative["text"]) ?? string.Empty,
                    Convert.ToBoolean(alternative["isCorrect"])))
                .ToList()
            : [];

        return Flashcard.Reconstitute(
            snapshot.Id,
            snapshot.GetValue<string>("deckId"),
            Enum.Parse<FlashcardType>(snapshot.GetValue<string>("type")),
            Enum.Parse<FlashcardDifficulty>(snapshot.GetValue<string>("difficulty")),
            snapshot.ContainsField("subarea") ? snapshot.GetValue<string?>("subarea") : null,
            hints,
            snapshot.ContainsField("question") ? snapshot.GetValue<string?>("question") : null,
            snapshot.ContainsField("answer") ? snapshot.GetValue<string?>("answer") : null,
            snapshot.ContainsField("clozeText") ? snapshot.GetValue<string?>("clozeText") : null,
            validAnswers,
            alternatives,
            snapshot.GetValue<Timestamp>("createdAt").ToDateTime(),
            snapshot.GetValue<Timestamp>("updatedAt").ToDateTime());
    }

    private static Dictionary<string, object?> ToDocument(Flashcard flashcard) => new()
    {
        ["deckId"] = flashcard.DeckId,
        ["type"] = flashcard.Type.ToString(),
        ["difficulty"] = flashcard.Difficulty.ToString(),
        ["subarea"] = flashcard.Subarea,
        ["hints"] = flashcard.Hints.ToList(),
        ["question"] = flashcard.Question,
        ["answer"] = flashcard.Answer,
        ["clozeText"] = flashcard.ClozeText,
        ["validAnswers"] = flashcard.ValidAnswers.ToList(),
        ["alternatives"] = flashcard.Alternatives
            .Select(alternative => new Dictionary<string, object?>
            {
                ["text"] = alternative.Text,
                ["isCorrect"] = alternative.IsCorrect
            })
            .ToList(),
        ["createdAt"] = Timestamp.FromDateTime(DateTime.SpecifyKind(flashcard.CreatedAt, DateTimeKind.Utc)),
        ["updatedAt"] = Timestamp.FromDateTime(DateTime.SpecifyKind(flashcard.UpdatedAt, DateTimeKind.Utc))
    };
}