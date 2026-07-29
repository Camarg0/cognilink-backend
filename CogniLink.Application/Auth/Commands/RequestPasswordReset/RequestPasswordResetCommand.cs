using MediatR;

namespace CogniLink.Application.Auth.Commands.RequestPasswordReset;

public sealed record RequestPasswordResetCommand(string Email) : IRequest;
