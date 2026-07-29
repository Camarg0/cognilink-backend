using FluentValidation;

namespace CogniLink.Application.Profile.Commands.UpdateProfile;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Theme).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ProfilePhotoUrl).MaximumLength(2048);
    }
}
