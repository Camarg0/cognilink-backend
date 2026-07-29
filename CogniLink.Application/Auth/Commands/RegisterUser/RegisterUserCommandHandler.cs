using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Models;
using CogniLink.Domain.Entities;
using MediatR;

namespace CogniLink.Application.Auth.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthTokensResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthTokensResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException("Não foi possível concluir o cadastro com os dados informados.");
        }

        var user = User.Create(Guid.NewGuid().ToString("N"), request.Name, request.Email, _passwordHasher.Hash(request.Password));
        await _userRepository.AddAsync(user, cancellationToken);

        var refreshToken = RefreshToken.Create(Guid.NewGuid().ToString("N"), user.Id, TimeSpan.FromDays(30));
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user.Id, user.Email);
        return new AuthTokensResponse(accessToken, refreshToken.Token, _jwtTokenGenerator.AccessTokenExpirationSeconds);
    }
}
