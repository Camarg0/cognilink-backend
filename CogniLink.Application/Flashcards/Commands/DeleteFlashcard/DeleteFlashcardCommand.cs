using MediatR;

namespace CogniLink.Application.Flashcards.Commands.DeleteFlashcard;

public sealed record DeleteFlashcardCommand(string Id) : IRequest;