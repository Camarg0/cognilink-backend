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

public sealed record FlashcardAlternativeRequest(string Text, bool IsCorrect);

public sealed record CreateFlashcardRequest(
	FlashcardType Type,
	FlashcardDifficulty Difficulty,
	string? Subarea,
	List<string>? Hints,
	string? Question,
	string? Answer,
	string? ClozeText,
	List<string>? ValidAnswers,
	List<FlashcardAlternativeRequest>? Alternatives);

public sealed record UpdateFlashcardRequest(
	FlashcardType Type,
	FlashcardDifficulty Difficulty,
	string? Subarea,
	List<string>? Hints,
	string? Question,
	string? Answer,
	string? ClozeText,
	List<string>? ValidAnswers,
	List<FlashcardAlternativeRequest>? Alternatives);

public sealed record StartStudySessionRequest(string DeckId);

public sealed record SubmitStudyAnswerRequest(
	Guid AttemptId,
	bool IsCorrect,
	int TimeToAnswerSeconds,
	int HintsViewed);
