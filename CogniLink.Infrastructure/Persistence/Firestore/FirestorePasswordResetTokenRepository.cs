using CogniLink.Application.Common.Interfaces;
using CogniLink.Domain.Entities;
using Google.Cloud.Firestore;

namespace CogniLink.Infrastructure.Persistence.Firestore;

public sealed class FirestorePasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private const string CollectionName = "passwordResetTokens";
    private readonly FirestoreDb _firestoreDb;

    public FirestorePasswordResetTokenRepository(FirestoreDb firestoreDb) => _firestoreDb = firestoreDb;

    public async Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        var snapshot = await _firestoreDb.Collection(CollectionName).Document(token).GetSnapshotAsync(cancellationToken);
        return snapshot.Exists ? ToDomain(snapshot) : null;
    }

    public Task AddAsync(PasswordResetToken resetToken, CancellationToken cancellationToken) =>
        _firestoreDb.Collection(CollectionName).Document(resetToken.Token)
            .SetAsync(ToDocument(resetToken), cancellationToken: cancellationToken);

    public Task UpdateAsync(PasswordResetToken resetToken, CancellationToken cancellationToken) =>
        _firestoreDb.Collection(CollectionName).Document(resetToken.Token)
            .SetAsync(ToDocument(resetToken), cancellationToken: cancellationToken);

    private static PasswordResetToken ToDomain(DocumentSnapshot snapshot) => PasswordResetToken.Reconstitute(
        snapshot.Id,
        snapshot.GetValue<string>("userId"),
        snapshot.GetValue<Timestamp>("expiresAt").ToDateTime(),
        snapshot.GetValue<bool>("isUsed"),
        snapshot.GetValue<Timestamp>("createdAt").ToDateTime());

    private static Dictionary<string, object?> ToDocument(PasswordResetToken resetToken) => new()
    {
        ["userId"] = resetToken.UserId,
        ["expiresAt"] = Timestamp.FromDateTime(DateTime.SpecifyKind(resetToken.ExpiresAt, DateTimeKind.Utc)),
        ["isUsed"] = resetToken.IsUsed,
        ["createdAt"] = Timestamp.FromDateTime(DateTime.SpecifyKind(resetToken.CreatedAt, DateTimeKind.Utc))
    };
}
