using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.StudySessions.Queries.GetNextCard;

public sealed record GetNextCardQuery(string SessionId) : IRequest<NextStudyCardDto?>;