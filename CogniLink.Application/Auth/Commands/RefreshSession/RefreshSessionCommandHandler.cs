using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Models;
using CogniLink.Domain.Entities;
using MediatR;

namespace CogniLink.Application.Auth.Commands.RefreshSession;

public sealed class RefreshSessionCommandHandler : IRequestHandler<RefreshSessionCommand, AuthTokensResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshSessionCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthTokensResponse> Handle(RefreshSessionCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (existingToken is null || !existingToken.IsValid)
        {
            throw new AuthenticationFailedException("Sessão expirada. Faça login novamente.");
        }

        existingToken.MarkUsed();
        await _refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);

        var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken)
            ?? throw new AuthenticationFailedException("Sessão expirada. Faça login novamente.");

        var newRefreshToken = RefreshToken.Create(Guid.NewGuid().ToString("N"), user.Id, TimeSpan.FromDays(30));
        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user.Id, user.Email);
        return new AuthTokensResponse(accessToken, newRefreshToken.Token, _jwtTokenGenerator.AccessTokenExpirationSeconds);
    }
}
