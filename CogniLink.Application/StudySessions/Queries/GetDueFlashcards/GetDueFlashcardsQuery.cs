using CogniLink.Application.Common.Models;
using MediatR;

namespace CogniLink.Application.StudySessions.Queries.GetDueFlashcards;

public sealed record GetDueFlashcardsQuery : IRequest<IReadOnlyList<DueFlashcardDto>>;