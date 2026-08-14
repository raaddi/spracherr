namespace Spracher.Modules.Vocabulary.Domain;

public sealed class Concept
{
    private Concept()
    {
    }

    internal Concept(
        Guid id,
        string key,
        VocabularyVisibility visibility,
        VocabularySourceType sourceType,
        string? sourceReference,
        PublicationStatus publicationStatus,
        Guid? ownerUserId,
        DateTimeOffset createdAt)
    {
        Id = id;
        Key = key;
        Visibility = visibility;
        SourceType = sourceType;
        SourceReference = sourceReference;
        PublicationStatus = publicationStatus;
        OwnerUserId = ownerUserId;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public string Key { get; private set; } = string.Empty;

    public VocabularyVisibility Visibility { get; private set; }

    public VocabularySourceType SourceType { get; private set; }

    public string? SourceReference { get; private set; }

    public PublicationStatus PublicationStatus { get; private set; }

    public Guid? OwnerUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static Concept CreatePrivate(
        Guid ownerUserId,
        string key,
        DateTimeOffset createdAt)
    {
        if (ownerUserId == Guid.Empty)
        {
            throw new ArgumentException("Owner user ID cannot be empty.", nameof(ownerUserId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return new Concept(
            Guid.CreateVersion7(),
            key.Trim(),
            VocabularyVisibility.Private,
            VocabularySourceType.UserCreated,
            sourceReference: null,
            PublicationStatus.Draft,
            ownerUserId,
            createdAt);
    }
}
