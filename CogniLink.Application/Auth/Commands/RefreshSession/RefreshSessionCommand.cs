using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.Auth.Commands.RefreshSession;

public sealed record RefreshSessionCommand(string RefreshToken) : IRequest<AuthTokensResponse>;
