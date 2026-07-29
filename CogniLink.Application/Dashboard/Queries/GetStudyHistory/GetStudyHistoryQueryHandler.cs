using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.Dashboard.Queries.GetStudyHistory;

public sealed class GetStudyHistoryQueryHandler : IRequestHandler<GetStudyHistoryQuery, IReadOnlyList<StudyHistoryEntryDto>>
{
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly IStudyAnswerRepository _studyAnswerRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetStudyHistoryQueryHandler(
        IStudySessionRepository studySessionRepository,
        IStudyAnswerRepository studyAnswerRepository,
        ICurrentUserService currentUserService)
    {
        _studySessionRepository = studySessionRepository;
        _studyAnswerRepository = studyAnswerRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<StudyHistoryEntryDto>> Handle(GetStudyHistoryQuery request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUserService.UserId ?? throw new AuthenticationFailedException();

        var from = request.From!.Value;
        var to = request.To!.Value;

        var sessions = await _studySessionRepository.ListByOwnerAsync(ownerId, cancellationToken);

        var answersByDate = new Dictionary<DateOnly, (int AnswersCount, int CorrectCount, long TotalTimeSeconds)>();

        foreach (var session in sessions)
        {
            var answers = await _studyAnswerRepository.ListBySessionIdAsync(session.Id, cancellationToken);

            foreach (var answer in answers)
            {
                var answeredDate = DateOnly.FromDateTime(answer.AnsweredAt);
                if (answeredDate < from || answeredDate > to)
                {
                    continue;
                }

                var current = answersByDate.TryGetValue(answeredDate, out var existing)
                    ? existing
                    : (AnswersCount: 0, CorrectCount: 0, TotalTimeSeconds: 0L);

                answersByDate[answeredDate] = (
                    current.AnswersCount + 1,
                    current.CorrectCount + (answer.IsCorrect ? 1 : 0),
                    current.TotalTimeSeconds + answer.TimeToAnswerSeconds);
            }
        }

        var history = new List<StudyHistoryEntryDto>();
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            var (answersCount, correctCount, totalTimeSeconds) = answersByDate.TryGetValue(date, out var entry)
                ? entry
                : (0, 0, 0L);

            history.Add(new StudyHistoryEntryDto(date, answersCount, correctCount, answersCount - correctCount, totalTimeSeconds));
        }

        return history;
    }
}
