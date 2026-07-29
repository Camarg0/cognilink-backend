namespace CogniLink.Application.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string message = "Recurso não encontrado.") : base(message)
    {
    }
}
