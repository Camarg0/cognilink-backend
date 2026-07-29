namespace CogniLink.Application.Common.Models;

public sealed record AuthTokensResponse(string AccessToken, string RefreshToken, int ExpiresIn);
