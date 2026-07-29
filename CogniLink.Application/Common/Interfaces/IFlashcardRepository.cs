using CogniLink.Domain.Entities;

namespace CogniLink.Application.Common.Interfaces;

public interface IFlashcardRepository
{
    Task AddAsync(Flashcard flashcard, CancellationToken cancellationToken);

    Task<Flashcard?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Flashcard>> ListByDeckIdAsync(string deckId, CancellationToken cancellationToken);

    Task UpdateAsync(Flashcard flashcard, CancellationToken cancellationToken);

    Task DeleteAsync(string id, CancellationToken cancellationToken);
}