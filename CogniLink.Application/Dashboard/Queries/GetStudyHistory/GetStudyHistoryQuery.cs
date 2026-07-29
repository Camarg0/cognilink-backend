using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.Dashboard.Queries.GetStudyHistory;

public sealed record GetStudyHistoryQuery(DateOnly? From, DateOnly? To) : IRequest<IReadOnlyList<StudyHistoryEntryDto>>;
