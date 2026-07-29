using MediatR;

namespace CogniLink.Application.Auth.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest;
