namespace CogniLink.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    int AccessTokenExpirationSeconds { get; }

    string GenerateAccessToken(string userId, string email);
}
