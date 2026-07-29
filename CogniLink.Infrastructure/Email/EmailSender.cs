using CogniLink.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CogniLink.Infrastructure.Email;

// PENDÊNCIA: substituir por integração real com um provedor de e-mail transacional antes de produção.
public sealed class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(ILogger<EmailSender> logger) => _logger = logger;

    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Password reset requested for a registered account. Token generated and pending delivery via a transactional e-mail provider.");
        return Task.CompletedTask;
    }
}
