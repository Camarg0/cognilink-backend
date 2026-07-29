using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.Flashcards.Queries.ListFlashcardsByDeck;

public sealed record ListFlashcardsByDeckQuery(string DeckId) : IRequest<IReadOnlyList<FlashcardDto>>;