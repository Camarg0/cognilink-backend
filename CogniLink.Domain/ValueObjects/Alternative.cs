namespace CogniLink.Domain.ValueObjects;

public sealed class Alternative
{
    public string Text { get; private set; } = string.Empty;
    public bool IsCorrect { get; private set; }

    private Alternative()
    {
    }

    public static Alternative Create(string text, bool isCorrect)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text is required.", nameof(text));
        }

        return new Alternative
        {
            Text = text.Trim(),
            IsCorrect = isCorrect
        };
    }

    public static Alternative Reconstitute(string text, bool isCorrect)
    {
        return Create(text, isCorrect);
    }
}
