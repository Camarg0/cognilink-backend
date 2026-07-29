using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.Decks.Queries.GetDeckById;

public sealed record GetDeckByIdQuery(string Id) : IRequest<DeckResponse>;
