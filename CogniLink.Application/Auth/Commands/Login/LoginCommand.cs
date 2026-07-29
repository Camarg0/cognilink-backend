using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthTokensResponse>;
