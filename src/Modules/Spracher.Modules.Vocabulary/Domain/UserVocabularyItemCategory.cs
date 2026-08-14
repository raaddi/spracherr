namespace Spracher.Modules.Vocabulary.Domain;

public sealed class UserVocabularyItemCategory
{
    private UserVocabularyItemCategory()
    {
    }

    private UserVocabularyItemCategory(
        Guid userVocabularyItemId,
        Guid vocabularyCategoryId,
        DateTimeOffset assignedAt)
    {
        UserVocabularyItemId = userVocabularyItemId;
        VocabularyCategoryId = vocabularyCategoryId;
        AssignedAt = assignedAt;
    }

    public Guid UserVocabularyItemId { get; private set; }

    public Guid VocabularyCategoryId { get; private set; }

    public DateTimeOffset AssignedAt { get; private set; }

    public static UserVocabularyItemCategory Create(
        Guid userVocabularyItemId,
        Guid vocabularyCategoryId,
        DateTimeOffset assignedAt)
    {
        if (userVocabularyItemId == Guid.Empty)
        {
            throw new ArgumentException(
                "User vocabulary item ID cannot be empty.",
                nameof(userVocabularyItemId));
        }

        if (vocabularyCategoryId == Guid.Empty)
        {
            throw new ArgumentException(
                "Vocabulary category ID cannot be empty.",
                nameof(vocabularyCategoryId));
        }

        return new UserVocabularyItemCategory(
            userVocabularyItemId,
            vocabularyCategoryId,
            assignedAt);
    }
}
