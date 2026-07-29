using CogniLink.Domain.Entities;

namespace CogniLink.Application.Common.Interfaces;

public interface IDeckRepository
{
    Task<Deck?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Deck>> GetByOwnerAsync(string ownerId, CancellationToken cancellationToken);

    Task AddAsync(Deck deck, CancellationToken cancellationToken);

    Task UpdateAsync(Deck deck, CancellationToken cancellationToken);

    Task DeleteAsync(string id, CancellationToken cancellationToken);
}
