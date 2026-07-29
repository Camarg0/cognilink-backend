namespace CogniLink.Domain.Entities;

public sealed class PasswordResetToken
{
    public string Token { get; private set; } = string.Empty;
    public string UserId { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PasswordResetToken()
    {
    }

    public bool IsValid => !IsUsed && DateTime.UtcNow < ExpiresAt;

    public static PasswordResetToken Create(string token, string userId, TimeSpan lifetime)
    {
        return new PasswordResetToken
        {
            Token = token,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(lifetime)
        };
    }

    public static PasswordResetToken Reconstitute(
        string token,
        string userId,
        DateTime expiresAt,
        bool isUsed,
        DateTime createdAt)
    {
        return new PasswordResetToken
        {
            Token = token,
            UserId = userId,
            ExpiresAt = expiresAt,
            IsUsed = isUsed,
            CreatedAt = createdAt
        };
    }

    public void MarkUsed() => IsUsed = true;
}
