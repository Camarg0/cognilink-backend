using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.Flashcards.Queries.GetFlashcardById;

public sealed record GetFlashcardByIdQuery(string Id) : IRequest<FlashcardDto>;