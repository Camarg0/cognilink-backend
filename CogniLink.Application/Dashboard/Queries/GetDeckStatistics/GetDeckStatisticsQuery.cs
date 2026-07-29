using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.Dashboard.Queries.GetDeckStatistics;

public sealed record GetDeckStatisticsQuery(string DeckId) : IRequest<DeckStatisticsDto>;
