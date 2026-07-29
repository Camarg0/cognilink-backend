using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.Decks.Queries.ListDecks;

public sealed record ListDecksQuery(string? Search, int Page = 1, int PageSize = 10) : IRequest<PagedResult<DeckResponse>>;
