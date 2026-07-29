using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.Profile.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(string Name, string Email, string? ProfilePhotoUrl, string Theme)
    : IRequest<ProfileResponse>;
