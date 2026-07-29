using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using MediatR;

namespace CogniLink.Application.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _resetTokenRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordResetTokenRepository resetTokenRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _resetTokenRepository = resetTokenRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var resetToken = await _resetTokenRepository.GetByTokenAsync(request.Token, cancellationToken);
        if (resetToken is null || !resetToken.IsValid)
        {
            throw new AuthenticationFailedException("Token de redefinição inválido ou expirado.");
        }

        var user = await _userRepository.GetByIdAsync(resetToken.UserId, cancellationToken)
            ?? throw new AuthenticationFailedException("Token de redefinição inválido ou expirado.");

        user.SetPasswordHash(_passwordHasher.Hash(request.NewPassword));
        await _userRepository.UpdateAsync(user, cancellationToken);

        resetToken.MarkUsed();
        await _resetTokenRepository.UpdateAsync(resetToken, cancellationToken);
    }
}
