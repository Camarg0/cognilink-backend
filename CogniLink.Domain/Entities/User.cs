namespace CogniLink.Domain.Entities;

public sealed class User
{
    public string Id { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? ProfilePhotoUrl { get; private set; }
    public UserPreference Preference { get; private set; } = UserPreference.Default();
    public DateTime CreatedAt { get; private set; }

    private User()
    {
    }

    public static User Create(string id, string name, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        return new User
        {
            Id = id,
            Name = name,
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Preference = UserPreference.Default(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static User Reconstitute(
        string id,
        string name,
        string email,
        string passwordHash,
        string? profilePhotoUrl,
        UserPreference preference,
        DateTime createdAt)
    {
        return new User
        {
            Id = id,
            Name = name,
            Email = email,
            PasswordHash = passwordHash,
            ProfilePhotoUrl = profilePhotoUrl,
            Preference = preference,
            CreatedAt = createdAt
        };
    }

    public void UpdateProfile(string name, string email, string? profilePhotoUrl, string theme)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));

        Name = name;
        Email = email.Trim().ToLowerInvariant();
        ProfilePhotoUrl = profilePhotoUrl;
        Preference = Preference.WithTheme(theme);
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        PasswordHash = passwordHash;
    }
}
