namespace CogniLink.Api.Contracts;

using CogniLink.Domain.Enums;

public sealed record RegisterRequest(string Name, string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshRequest(string RefreshToken);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(string Token, string NewPassword);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed record UpdateProfileRequest(string Name, string Email, string? ProfilePhotoUrl, string Theme);

public sealed record CreateDeckRequest(string Name, string? Description, DeckDifficulty Difficulty, List<string>? Categories);

public sealed record UpdateDeckRequest(string Name, string? Description, DeckDifficulty Difficulty, List<string>? Categories);
