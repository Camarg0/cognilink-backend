using System.Security.Cryptography;
using System.Text;

namespace CogniLink.Application.StudySessions;

public static class AttemptIdFactory
{
    public static Guid Create(string sessionId, string flashcardId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("SessionId is required.", nameof(sessionId));
        }

        if (string.IsNullOrWhiteSpace(flashcardId))
        {
            throw new ArgumentException("FlashcardId is required.", nameof(flashcardId));
        }

        var source = Encoding.UTF8.GetBytes($"{sessionId}:{flashcardId}");
        var hash = SHA256.HashData(source);
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);

        return new Guid(guidBytes);
    }
}