namespace CogniLink.Domain.Entities;

public sealed class RefreshToken
{
    public string Token { get; private set; } = string.Empty;
    public string UserId { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private RefreshToken()
    {
    }

    public bool IsValid => !IsUsed && !IsRevoked && DateTime.UtcNow < ExpiresAt;

    public static RefreshToken Create(string token, string userId, TimeSpan lifetime)
    {
        return new RefreshToken
        {
            Token = token,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(lifetime)
        };
    }

    public static RefreshToken Reconstitute(
        string token,
        string userId,
        DateTime expiresAt,
        bool isUsed,
        bool isRevoked,
        DateTime createdAt)
    {
        return new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiresAt = expiresAt,
            IsUsed = isUsed,
            IsRevoked = isRevoked,
            CreatedAt = createdAt
        };
    }

    public void MarkUsed() => IsUsed = true;

    public void Revoke() => IsRevoked = true;
}
