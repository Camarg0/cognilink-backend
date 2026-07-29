using CogniLink.Application.Common.Interfaces;
using MediatR;

namespace CogniLink.Application.Auth.Commands.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepository) =>
        _refreshTokenRepository = refreshTokenRepository;

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (token is not null && !token.IsRevoked)
        {
            token.Revoke();
            await _refreshTokenRepository.UpdateAsync(token, cancellationToken);
        }
    }
}
