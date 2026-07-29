using CogniLink.Application.Common.Interfaces;
using CogniLink.Domain.Entities;
using CogniLink.Domain.Enums;
using Google.Cloud.Firestore;

namespace CogniLink.Infrastructure.Persistence.Firestore;

public sealed class FirestoreDeckRepository : IDeckRepository
{
    private const string CollectionName = "decks";
    private readonly FirestoreDb _firestoreDb;

    public FirestoreDeckRepository(FirestoreDb firestoreDb) => _firestoreDb = firestoreDb;

    public async Task<Deck?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var snapshot = await _firestoreDb.Collection(CollectionName).Document(id).GetSnapshotAsync(cancellationToken);
        return snapshot.Exists ? ToDomain(snapshot) : null;
    }

    public async Task<IReadOnlyList<Deck>> GetByOwnerAsync(string ownerId, CancellationToken cancellationToken)
    {
        var query = _firestoreDb.Collection(CollectionName).WhereEqualTo("ownerId", ownerId);
        var snapshot = await query.GetSnapshotAsync(cancellationToken);
        return snapshot.Documents.Select(ToDomain).ToList();
    }

    public Task AddAsync(Deck deck, CancellationToken cancellationToken) =>
        _firestoreDb.Collection(CollectionName).Document(deck.Id).SetAsync(ToDocument(deck), cancellationToken: cancellationToken);

    public Task UpdateAsync(Deck deck, CancellationToken cancellationToken) =>
        _firestoreDb.Collection(CollectionName).Document(deck.Id).SetAsync(ToDocument(deck), cancellationToken: cancellationToken);

    public Task DeleteAsync(string id, CancellationToken cancellationToken) =>
        _firestoreDb.Collection(CollectionName).Document(id).DeleteAsync(cancellationToken: cancellationToken);

    private static Deck ToDomain(DocumentSnapshot snapshot)
    {
        var categories = snapshot.ContainsField("categories")
            ? snapshot.GetValue<List<string>>("categories")
            : [];

        return Deck.Reconstitute(
            snapshot.Id,
            snapshot.GetValue<string>("ownerId"),
            snapshot.GetValue<string>("name"),
            snapshot.ContainsField("description") ? snapshot.GetValue<string?>("description") : null,
            Enum.Parse<DeckDifficulty>(snapshot.GetValue<string>("difficulty")),
            categories,
            snapshot.GetValue<Timestamp>("createdAt").ToDateTime(),
            snapshot.GetValue<Timestamp>("updatedAt").ToDateTime());
    }

    private static Dictionary<string, object?> ToDocument(Deck deck) => new()
    {
        ["ownerId"] = deck.OwnerId,
        ["name"] = deck.Name,
        ["description"] = deck.Description,
        ["difficulty"] = deck.Difficulty.ToString(),
        ["categories"] = deck.Categories.ToList(),
        ["createdAt"] = Timestamp.FromDateTime(DateTime.SpecifyKind(deck.CreatedAt, DateTimeKind.Utc)),
        ["updatedAt"] = Timestamp.FromDateTime(DateTime.SpecifyKind(deck.UpdatedAt, DateTimeKind.Utc))
    };
}
