namespace CogniLink.Application.Common.Models;

public sealed record ProfileResponse(string Id, string Name, string Email, string? ProfilePhotoUrl, string Theme);
