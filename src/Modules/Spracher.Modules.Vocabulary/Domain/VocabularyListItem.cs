namespace Spracher.Modules.Vocabulary.Domain;

public sealed class VocabularyListItem
{
    private VocabularyListItem()
    {
    }

    private VocabularyListItem(
        Guid vocabularyListId,
        Guid lexemeSenseId,
        int position,
        string? note,
        DateTimeOffset addedAt)
    {
        VocabularyListId = vocabularyListId;
        LexemeSenseId = lexemeSenseId;
        Position = position;
        Note = note?.Trim();
        AddedAt = addedAt;
    }

    public Guid VocabularyListId { get; private set; }

    public Guid LexemeSenseId { get; private set; }

    public int Position { get; private set; }

    public string? Note { get; private set; }

    public DateTimeOffset AddedAt { get; private set; }

    public static VocabularyListItem Create(
        Guid vocabularyListId,
        Guid lexemeSenseId,
        int position,
        string? note,
        DateTimeOffset addedAt)
    {
        if (vocabularyListId == Guid.Empty)
        {
            throw new ArgumentException("Vocabulary list ID cannot be empty.", nameof(vocabularyListId));
        }

        if (lexemeSenseId == Guid.Empty)
        {
            throw new ArgumentException("Lexeme sense ID cannot be empty.", nameof(lexemeSenseId));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(position);

        if (note?.Length > 500)
        {
            throw new ArgumentException("List item note cannot exceed 500 characters.", nameof(note));
        }

        return new VocabularyListItem(
            vocabularyListId,
            lexemeSenseId,
            position,
            note,
            addedAt);
    }
}
