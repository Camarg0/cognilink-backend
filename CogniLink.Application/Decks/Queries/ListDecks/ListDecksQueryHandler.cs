using CogniLink.Application.Common.Exceptions;
using CogniLink.Application.Common.Interfaces;
using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.Decks.Queries.ListDecks;

public sealed class ListDecksQueryHandler : IRequestHandler<ListDecksQuery, PagedResult<DeckResponse>>
{
    private readonly IDeckRepository _deckRepository;
    private readonly ICurrentUserService _currentUserService;

    public ListDecksQueryHandler(IDeckRepository deckRepository, ICurrentUserService currentUserService)
    {
        _deckRepository = deckRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<DeckResponse>> Handle(ListDecksQuery request, CancellationToken cancellationToken)
    {
        var ownerId = _currentUserService.UserId ?? throw new AuthenticationFailedException();

        var decks = await _deckRepository.GetByOwnerAsync(ownerId, cancellationToken);

        var filtered = string.IsNullOrWhiteSpace(request.Search)
            ? decks
            : decks
                .Where(deck => deck.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();

        var totalCount = filtered.Count;

        var items = filtered
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(deck => new DeckResponse(
                deck.Id,
                deck.Name,
                deck.Description,
                deck.Difficulty.ToString(),
                deck.Categories,
                0,
                deck.CreatedAt,
                deck.UpdatedAt))
            .ToList();

        return new PagedResult<DeckResponse>(items, request.Page, request.PageSize, totalCount);
    }
}
