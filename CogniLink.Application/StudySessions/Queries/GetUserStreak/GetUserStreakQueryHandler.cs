using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.StudySessions.Queries.GetUserStreak;

public sealed class GetUserStreakQueryHandler : IRequestHandler<GetUserStreakQuery, UserStreakDto>
{
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUserStreakQueryHandler(
        IStudySessionRepository studySessionRepository,
        ICurrentUserService currentUserService)
    {
        _studySessionRepository = studySessionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<UserStreakDto> Handle(GetUserStreakQuery request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUserService.UserId ?? throw new AuthenticationFailedException();

        var completedSessions = await _studySessionRepository.ListCompletedByOwnerAsync(ownerId, cancellationToken);
        var studyDates = completedSessions
            .Where(session => session.CompletedAt.HasValue)
            .Select(session => DateOnly.FromDateTime(session.CompletedAt!.Value))
            .Distinct()
            .OrderBy(date => date)
            .ToList();

        if (studyDates.Count == 0)
        {
            return new UserStreakDto(0, 0, null);
        }

        var longestStreak = 1;
        var runningLongest = 1;

        for (var index = 1; index < studyDates.Count; index++)
        {
            if (studyDates[index] == studyDates[index - 1].AddDays(1))
            {
                runningLongest++;
                longestStreak = Math.Max(longestStreak, runningLongest);
            }
            else
            {
                runningLongest = 1;
            }
        }

        var currentStreak = 1;
        for (var index = studyDates.Count - 1; index > 0; index--)
        {
            if (studyDates[index] == studyDates[index - 1].AddDays(1))
            {
                currentStreak++;
                continue;
            }

            break;
        }

        return new UserStreakDto(currentStreak, longestStreak, studyDates[^1]);
    }
}