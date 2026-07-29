using CogniLink.Application.Common.Interfaces;
using CogniLink.Domain.Entities;
using Google.Cloud.Firestore;

namespace CogniLink.Infrastructure.Persistence.Firestore;

public sealed class FirestoreUserRepository : IUserRepository
{
    private const string CollectionName = "users";
    private readonly FirestoreDb _firestoreDb;

    public FirestoreUserRepository(FirestoreDb firestoreDb) => _firestoreDb = firestoreDb;

    public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var snapshot = await _firestoreDb.Collection(CollectionName).Document(id).GetSnapshotAsync(cancellationToken);
        return snapshot.Exists ? ToDomain(snapshot) : null;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var query = _firestoreDb.Collection(CollectionName).WhereEqualTo("email", normalizedEmail).Limit(1);
        var snapshot = await query.GetSnapshotAsync(cancellationToken);
        return snapshot.Count > 0 ? ToDomain(snapshot[0]) : null;
    }

    public Task AddAsync(User user, CancellationToken cancellationToken) =>
        _firestoreDb.Collection(CollectionName).Document(user.Id).SetAsync(ToDocument(user), cancellationToken: cancellationToken);

    public Task UpdateAsync(User user, CancellationToken cancellationToken) =>
        _firestoreDb.Collection(CollectionName).Document(user.Id).SetAsync(ToDocument(user), cancellationToken: cancellationToken);

    private static User ToDomain(DocumentSnapshot snapshot)
    {
        return User.Reconstitute(
            snapshot.Id,
            snapshot.GetValue<string>("name"),
            snapshot.GetValue<string>("email"),
            snapshot.GetValue<string>("passwordHash"),
            snapshot.ContainsField("profilePhotoUrl") ? snapshot.GetValue<string?>("profilePhotoUrl") : null,
            UserPreference.FromTheme(snapshot.ContainsField("theme") ? snapshot.GetValue<string>("theme") : null),
            snapshot.GetValue<Timestamp>("createdAt").ToDateTime());
    }

    private static Dictionary<string, object?> ToDocument(User user) => new()
    {
        ["name"] = user.Name,
        ["email"] = user.Email,
        ["passwordHash"] = user.PasswordHash,
        ["profilePhotoUrl"] = user.ProfilePhotoUrl,
        ["theme"] = user.Preference.Theme,
        ["createdAt"] = Timestamp.FromDateTime(DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc))
    };
}
