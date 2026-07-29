using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.Profile.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, ProfileResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateProfileCommandHandler(IUserRepository userRepository, ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ProfileResponse> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new AuthenticationFailedException();
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new AuthenticationFailedException();

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (!string.Equals(normalizedEmail, user.Email, StringComparison.Ordinal))
        {
            var existing = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
            if (existing is not null && existing.Id != user.Id)
            {
                throw new ConflictException("Não foi possível atualizar o perfil com os dados informados.");
            }
        }

        user.UpdateProfile(request.Name, normalizedEmail, request.ProfilePhotoUrl, request.Theme);
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new ProfileResponse(user.Id, user.Name, user.Email, user.ProfilePhotoUrl, user.Preference.Theme);
    }
}
