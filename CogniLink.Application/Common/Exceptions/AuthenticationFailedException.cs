namespace CogniLink.Application.Common.Exceptions;

public sealed class AuthenticationFailedException : Exception
{
    public AuthenticationFailedException(string message = "Credenciais inválidas.") : base(message)
    {
    }
}
