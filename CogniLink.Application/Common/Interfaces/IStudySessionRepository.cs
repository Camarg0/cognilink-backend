using CogniLink.Domain.Entities;

namespace CogniLink.Application.Common.Interfaces;

public interface IStudySessionRepository
{
    Task AddAsync(StudySession session, CancellationToken cancellationToken);

    Task<StudySession?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task<StudySession?> GetByIdAndOwnerAsync(string id, string ownerId, CancellationToken cancellationToken);

    /// <summary>
    /// Retorna as sessões do usuário em qualquer status. Use para agregações que consideram
    /// toda resposta já registrada, inclusive de sessões em andamento ou canceladas.
    /// </summary>
    Task<IReadOnlyList<StudySession>> ListByOwnerAsync(string ownerId, CancellationToken cancellationToken);

    /// <summary>
    /// Retorna somente as sessões concluídas do usuário. Base do cálculo de streak.
    /// </summary>
    Task<IReadOnlyList<StudySession>> ListCompletedByOwnerAsync(string ownerId, CancellationToken cancellationToken);

    Task UpdateAsync(StudySession session, CancellationToken cancellationToken);
}