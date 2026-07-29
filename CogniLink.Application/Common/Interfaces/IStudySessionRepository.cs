using CogniLink.Domain.Entities;

namespace CogniLink.Application.Common.Interfaces;

public interface IStudySessionRepository
{
    Task AddAsync(StudySession session, CancellationToken cancellationToken);

    Task<StudySession?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task<StudySession?> GetByIdAndOwnerAsync(string id, string ownerId, CancellationToken cancellationToken);

    Task<IReadOnlyList<StudySession>> ListCompletedByOwnerAsync(string ownerId, CancellationToken cancellationToken);

    Task UpdateAsync(StudySession session, CancellationToken cancellationToken);
}