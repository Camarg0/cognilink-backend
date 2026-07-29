using CogniLink.Domain.Enums;
using CogniLink.Domain.ValueObjects;

namespace CogniLink.Domain.Entities;

public sealed class Flashcard
{
    private const int SubareaMaxLength = 50;
    private const int HintsMaxItems = 3;
    private readonly List<string> _hints = [];
    private readonly List<string> _validAnswers = [];
    private readonly List<Alternative> _alternatives = [];

    public string Id { get; private set; } = string.Empty;
    public string DeckId { get; private set; } = string.Empty;
    public FlashcardType Type { get; private set; }
    public FlashcardDifficulty Difficulty { get; private set; }
    public string? Subarea { get; private set; }
    public IReadOnlyList<string> Hints => _hints;
    public string? Question { get; private set; }
    public string? Answer { get; private set; }
    public string? ClozeText { get; private set; }
    public IReadOnlyList<string> ValidAnswers => _validAnswers;
    public IReadOnlyList<Alternative> Alternatives => _alternatives;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Flashcard()
    {
    }

    public static Flashcard Create(
        string id,
        string deckId,
        FlashcardType type,
        FlashcardDifficulty difficulty,
        string? subarea,
        IReadOnlyList<string>? hints,
        string? question,
        string? answer,
        string? clozeText,
        IReadOnlyList<string>? validAnswers,
        IReadOnlyList<Alternative>? alternatives)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(deckId))
        {
            throw new ArgumentException("DeckId is required.", nameof(deckId));
        }

        var flashcard = new Flashcard
        {
            Id = id,
            DeckId = deckId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        flashcard.ApplyState(type, difficulty, subarea, hints, question, answer, clozeText, validAnswers, alternatives);
        flashcard.ValidateInvariants();

        return flashcard;
    }

    public static Flashcard Reconstitute(
        string id,
        string deckId,
        FlashcardType type,
        FlashcardDifficulty difficulty,
        string? subarea,
        IReadOnlyList<string>? hints,
        string? question,
        string? answer,
        string? clozeText,
        IReadOnlyList<string>? validAnswers,
        IReadOnlyList<Alternative>? alternatives,
        DateTime createdAt,
        DateTime updatedAt)
    {
        var flashcard = new Flashcard
        {
            Id = id,
            DeckId = deckId,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        flashcard.ApplyState(type, difficulty, subarea, hints, question, answer, clozeText, validAnswers, alternatives);
        flashcard.ValidateInvariants();

        return flashcard;
    }

    public void Update(
        FlashcardType type,
        FlashcardDifficulty difficulty,
        string? subarea,
        IReadOnlyList<string>? hints,
        string? question,
        string? answer,
        string? clozeText,
        IReadOnlyList<string>? validAnswers,
        IReadOnlyList<Alternative>? alternatives)
    {
        ApplyState(type, difficulty, subarea, hints, question, answer, clozeText, validAnswers, alternatives);
        ValidateInvariants();
        UpdatedAt = DateTime.UtcNow;
    }

    private void ApplyState(
        FlashcardType type,
        FlashcardDifficulty difficulty,
        string? subarea,
        IReadOnlyList<string>? hints,
        string? question,
        string? answer,
        string? clozeText,
        IReadOnlyList<string>? validAnswers,
        IReadOnlyList<Alternative>? alternatives)
    {
        Type = type;
        Difficulty = difficulty;

        Subarea = NormalizeOptionalString(subarea, SubareaMaxLength, nameof(subarea));

        _hints.Clear();
        if (hints is not null)
        {
            if (hints.Count > HintsMaxItems)
            {
                throw new ArgumentException($"Hints must contain at most {HintsMaxItems} items.", nameof(hints));
            }

            foreach (var hint in hints)
            {
                if (string.IsNullOrWhiteSpace(hint))
                {
                    throw new ArgumentException("Hints cannot contain empty values.", nameof(hints));
                }

                _hints.Add(hint.Trim());
            }
        }

        Question = null;
        Answer = null;
        ClozeText = null;
        _validAnswers.Clear();
        _alternatives.Clear();

        switch (type)
        {
            case FlashcardType.FrenteVerso:
                Question = NormalizeRequiredString(question, nameof(question), "Question");
                Answer = NormalizeRequiredString(answer, nameof(answer), "Answer");
                break;
            case FlashcardType.Cloze:
                ClozeText = NormalizeRequiredString(clozeText, nameof(clozeText), "ClozeText");
                break;
            case FlashcardType.DigiteResposta:
                if (validAnswers is null || validAnswers.Count == 0)
                {
                    throw new ArgumentException("ValidAnswers are required for DigiteResposta flashcards.", nameof(validAnswers));
                }

                foreach (var validAnswer in validAnswers)
                {
                    if (string.IsNullOrWhiteSpace(validAnswer))
                    {
                        throw new ArgumentException("ValidAnswers cannot contain empty values.", nameof(validAnswers));
                    }

                    _validAnswers.Add(validAnswer.Trim());
                }

                break;
            case FlashcardType.MultiplaEscolha:
                if (alternatives is null || alternatives.Count < 2 || alternatives.Count > 5)
                {
                    throw new ArgumentException("MultiplaEscolha flashcards must have between 2 and 5 alternatives.", nameof(alternatives));
                }

                foreach (var alternative in alternatives)
                {
                    _alternatives.Add(Alternative.Reconstitute(alternative.Text, alternative.IsCorrect));
                }

                if (_alternatives.Count(alternative => alternative.IsCorrect) != 1)
                {
                    throw new ArgumentException("MultiplaEscolha flashcards must contain exactly one correct alternative.", nameof(alternatives));
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported flashcard type.");
        }
    }

    internal void ValidateInvariants()
    {
        if (!Enum.IsDefined(Type))
        {
            throw new ArgumentOutOfRangeException(nameof(Type), Type, "Invalid flashcard type.");
        }

        if (!Enum.IsDefined(Difficulty))
        {
            throw new ArgumentOutOfRangeException(nameof(Difficulty), Difficulty, "Invalid flashcard difficulty.");
        }

        if (Type == FlashcardType.FrenteVerso)
        {
            if (string.IsNullOrWhiteSpace(Question))
            {
                throw new ArgumentException("Question is required for FrenteVerso flashcards.");
            }

            if (string.IsNullOrWhiteSpace(Answer))
            {
                throw new ArgumentException("Answer is required for FrenteVerso flashcards.");
            }

            return;
        }

        if (Type == FlashcardType.Cloze)
        {
            if (string.IsNullOrWhiteSpace(ClozeText))
            {
                throw new ArgumentException("ClozeText is required for Cloze flashcards.");
            }

            if (!ContainsSequentialClozePlaceholders(ClozeText))
            {
                throw new ArgumentException("ClozeText must contain sequential placeholders like {{c1::term}}.");
            }

            return;
        }

        if (Type == FlashcardType.DigiteResposta)
        {
            if (_validAnswers.Count == 0)
            {
                throw new ArgumentException("At least one valid answer is required for DigiteResposta flashcards.");
            }

            return;
        }

        if (Type == FlashcardType.MultiplaEscolha)
        {
            if (_alternatives.Count is < 2 or > 5)
            {
                throw new ArgumentException("MultiplaEscolha flashcards must have between 2 and 5 alternatives.");
            }

            if (_alternatives.Count(alternative => alternative.IsCorrect) != 1)
            {
                throw new ArgumentException("MultiplaEscolha flashcards must contain exactly one correct alternative.");
            }

            return;
        }
    }

    private static string NormalizeRequiredString(string? value, string paramName, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{fieldName} is required.", paramName);
        }

        return value.Trim();
    }

    private static string? NormalizeOptionalString(string? value, int maxLength, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmedValue = value.Trim();
        if (trimmedValue.Length > maxLength)
        {
            throw new ArgumentException($"Value must be at most {maxLength} characters.", paramName);
        }

        return trimmedValue;
    }

    private static bool ContainsSequentialClozePlaceholders(string clozeText)
    {
        var matches = System.Text.RegularExpressions.Regex.Matches(clozeText, @"\{\{c(\d+)::([^{}]+)\}\}");
        if (matches.Count == 0)
        {
            return false;
        }

        var indices = matches
            .Cast<System.Text.RegularExpressions.Match>()
            .Select(match => int.Parse(match.Groups[1].Value))
            .OrderBy(index => index)
            .ToArray();

        if (indices[0] != 1)
        {
            return false;
        }

        for (var index = 1; index < indices.Length; index++)
        {
            if (indices[index] != indices[index - 1] + 1)
            {
                return false;
            }
        }

        return true;
    }
}
