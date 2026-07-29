namespace CogniLink.Application.Common.Models;

public sealed record DeckResponse(
    string Id,
    string Name,
    string? Description,
    string Difficulty,
    IReadOnlyList<string> Categories,
    int CardCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
