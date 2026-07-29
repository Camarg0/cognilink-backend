using CogniLink.Domain.Entities;

namespace CogniLink.Application.Common.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);

    Task AddAsync(PasswordResetToken resetToken, CancellationToken cancellationToken);

    Task UpdateAsync(PasswordResetToken resetToken, CancellationToken cancellationToken);
}
