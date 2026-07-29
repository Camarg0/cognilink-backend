using CogniLink.Application.Common.Interfaces;
using CogniLink.Domain.Entities;
using MediatR;

namespace CogniLink.Application.Auth.Commands.RequestPasswordReset;

public sealed class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _resetTokenRepository;
    private readonly IEmailSender _emailSender;

    public RequestPasswordResetCommandHandler(
        IUserRepository userRepository,
        IPasswordResetTokenRepository resetTokenRepository,
        IEmailSender emailSender)
    {
        _userRepository = userRepository;
        _resetTokenRepository = resetTokenRepository;
        _emailSender = emailSender;
    }

    public async Task Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            // Resposta genérica: não revela se o e-mail está cadastrado.
            return;
        }

        var resetToken = PasswordResetToken.Create(Guid.NewGuid().ToString("N"), user.Id, TimeSpan.FromHours(1));
        await _resetTokenRepository.AddAsync(resetToken, cancellationToken);
        await _emailSender.SendPasswordResetEmailAsync(user.Email, resetToken.Token, cancellationToken);
    }
}
