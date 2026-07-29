using CogniLink.Domain.Entities;

namespace CogniLink.Application.Common.Interfaces;

public interface IUserFlashcardProgressRepository
{
    Task<UserFlashcardProgress?> GetByOwnerAndFlashcardAsync(string ownerId, string flashcardId, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserFlashcardProgress>> ListByOwnerAsync(string ownerId, CancellationToken cancellationToken);

    Task UpsertAsync(UserFlashcardProgress progress, CancellationToken cancellationToken);
}