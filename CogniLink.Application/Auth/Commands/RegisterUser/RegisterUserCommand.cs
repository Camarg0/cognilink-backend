using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.Auth.Commands.RegisterUser;

public sealed record RegisterUserCommand(string Name, string Email, string Password) : IRequest<AuthTokensResponse>;
