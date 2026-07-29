using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.Dashboard.Queries.GetDashboard;

public sealed record GetDashboardQuery : IRequest<DashboardSummaryDto>;
