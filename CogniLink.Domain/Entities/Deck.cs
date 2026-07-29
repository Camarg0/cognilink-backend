using CogniLink.Domain.Enums;

namespace CogniLink.Domain.Entities;

public sealed class Deck
{
    private const int NameMinLength = 3;
    private const int NameMaxLength = 100;
    private const int DescriptionMaxLength = 500;
    private const int MaxCategories = 5;
    private const int CategoryMaxLength = 30;

    private readonly List<string> _categories = [];

    public string Id { get; private set; } = string.Empty;
    public string OwnerId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DeckDifficulty Difficulty { get; private set; }
    public IReadOnlyList<string> Categories => _categories;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Deck()
    {
    }

    public static Deck Create(
        string id,
        string ownerId,
        string name,
        string? description,
        DeckDifficulty difficulty,
        IReadOnlyList<string>? categories)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("OwnerId is required.", nameof(ownerId));

        var deck = new Deck
        {
            Id = id,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow
        };

        deck.UpdatedAt = deck.CreatedAt;
        deck.SetDetails(name, description, difficulty, categories);

        return deck;
    }

    public static Deck Reconstitute(
        string id,
        string ownerId,
        string name,
        string? description,
        DeckDifficulty difficulty,
        IReadOnlyList<string>? categories,
        DateTime createdAt,
        DateTime updatedAt)
    {
        var deck = new Deck
        {
            Id = id,
            OwnerId = ownerId,
            Name = name,
            Description = description,
            Difficulty = difficulty,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        deck._categories.AddRange(categories ?? []);

        return deck;
    }

    public void UpdateDetails(
        string name,
        string? description,
        DeckDifficulty difficulty,
        IReadOnlyList<string>? categories)
    {
        SetDetails(name, description, difficulty, categories);
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetDetails(
        string name,
        string? description,
        DeckDifficulty difficulty,
        IReadOnlyList<string>? categories)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));

        var trimmedName = name.Trim();
        if (trimmedName.Length < NameMinLength || trimmedName.Length > NameMaxLength)
        {
            throw new ArgumentException($"Name must be between {NameMinLength} and {NameMaxLength} characters.", nameof(name));
        }

        if (description is not null && description.Length > DescriptionMaxLength)
        {
            throw new ArgumentException($"Description must be at most {DescriptionMaxLength} characters.", nameof(description));
        }

        var categoryList = categories ?? [];
        if (categoryList.Count > MaxCategories)
        {
            throw new ArgumentException($"Categories must contain at most {MaxCategories} items.", nameof(categories));
        }

        if (categoryList.Any(category => category.Length > CategoryMaxLength))
        {
            throw new ArgumentException($"Each category must be at most {CategoryMaxLength} characters.", nameof(categories));
        }

        Name = trimmedName;
        Description = description;
        Difficulty = difficulty;
        _categories.Clear();
        _categories.AddRange(categoryList);
    }
}
