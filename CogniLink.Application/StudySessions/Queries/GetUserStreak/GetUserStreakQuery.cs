using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.StudySessions.Queries.GetUserStreak;

public sealed record GetUserStreakQuery : IRequest<UserStreakDto>;