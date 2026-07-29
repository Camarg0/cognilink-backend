using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.Profile.Queries.GetProfile;

public sealed record GetProfileQuery : IRequest<ProfileResponse>;
