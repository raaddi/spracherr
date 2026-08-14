namespace Spracher.Modules.Vocabulary.Domain;

public sealed class UserVocabularyItem
{
    private UserVocabularyItem()
    {
    }

    private UserVocabularyItem(
        Guid userId,
        Guid lexemeSenseId,
        DateTimeOffset addedAt)
    {
        Id = Guid.CreateVersion7();
        UserId = userId;
        LexemeSenseId = lexemeSenseId;
        Status = UserVocabularyStatus.New;
        AddedAt = addedAt;
        StatusChangedAt = addedAt;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid LexemeSenseId { get; private set; }

    public UserVocabularyStatus Status { get; private set; }

    public DateTimeOffset AddedAt { get; private set; }

    public DateTimeOffset StatusChangedAt { get; private set; }

    public static UserVocabularyItem Create(
        Guid userId,
        Guid lexemeSenseId,
        DateTimeOffset addedAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        if (lexemeSenseId == Guid.Empty)
        {
            throw new ArgumentException("Lexeme sense ID cannot be empty.", nameof(lexemeSenseId));
        }

        return new UserVocabularyItem(userId, lexemeSenseId, addedAt);
    }

    public void ChangeStatus(UserVocabularyStatus status, DateTimeOffset changedAt)
    {
        if (status == Status)
        {
            return;
        }

        Status = status;
        StatusChangedAt = changedAt;
    }
}
