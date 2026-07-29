using CogniLink.Application.Common.Interfaces;
using CogniLink.Domain.Entities;
using Google.Cloud.Firestore;

namespace CogniLink.Infrastructure.Persistence.Firestore;

public sealed class FirestoreRefreshTokenRepository : IRefreshTokenRepository
{
    private const string CollectionName = "refreshTokens";
    private readonly FirestoreDb _firestoreDb;

    public FirestoreRefreshTokenRepository(FirestoreDb firestoreDb) => _firestoreDb = firestoreDb;

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        var snapshot = await _firestoreDb.Collection(CollectionName).Document(token).GetSnapshotAsync(cancellationToken);
        return snapshot.Exists ? ToDomain(snapshot) : null;
    }

    public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken) =>
        _firestoreDb.Collection(CollectionName).Document(refreshToken.Token)
            .SetAsync(ToDocument(refreshToken), cancellationToken: cancellationToken);

    public Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken) =>
        _firestoreDb.Collection(CollectionName).Document(refreshToken.Token)
            .SetAsync(ToDocument(refreshToken), cancellationToken: cancellationToken);

    private static RefreshToken ToDomain(DocumentSnapshot snapshot) => RefreshToken.Reconstitute(
        snapshot.Id,
        snapshot.GetValue<string>("userId"),
        snapshot.GetValue<Timestamp>("expiresAt").ToDateTime(),
        snapshot.GetValue<bool>("isUsed"),
        snapshot.GetValue<bool>("isRevoked"),
        snapshot.GetValue<Timestamp>("createdAt").ToDateTime());

    private static Dictionary<string, object?> ToDocument(RefreshToken refreshToken) => new()
    {
        ["userId"] = refreshToken.UserId,
        ["expiresAt"] = Timestamp.FromDateTime(DateTime.SpecifyKind(refreshToken.ExpiresAt, DateTimeKind.Utc)),
        ["isUsed"] = refreshToken.IsUsed,
        ["isRevoked"] = refreshToken.IsRevoked,
        ["createdAt"] = Timestamp.FromDateTime(DateTime.SpecifyKind(refreshToken.CreatedAt, DateTimeKind.Utc))
    };
}
