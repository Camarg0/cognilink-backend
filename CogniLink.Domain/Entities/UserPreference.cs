namespace CogniLink.Domain.Entities;

public sealed class UserPreference
{
    public const string DefaultTheme = "system";

    public string Theme { get; private set; } = DefaultTheme;

    private UserPreference()
    {
    }

    public static UserPreference Default() => new() { Theme = DefaultTheme };

    public static UserPreference FromTheme(string? theme) =>
        new() { Theme = string.IsNullOrWhiteSpace(theme) ? DefaultTheme : theme };

    public UserPreference WithTheme(string theme) => FromTheme(theme);
}
