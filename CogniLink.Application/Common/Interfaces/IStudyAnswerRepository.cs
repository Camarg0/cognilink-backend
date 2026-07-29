using CogniLink.Domain.Entities;

namespace CogniLink.Application.Common.Interfaces;

public interface IStudyAnswerRepository
{
    Task<StudyAnswer?> GetByAttemptIdAsync(Guid attemptId, CancellationToken cancellationToken);

    Task<IReadOnlyList<StudyAnswer>> ListBySessionIdAsync(string sessionId, CancellationToken cancellationToken);

    Task AddAsync(StudyAnswer answer, CancellationToken cancellationToken);
}