namespace Spracher.Modules.Vocabulary.Domain;

public sealed class ExampleSentence
{
    private ExampleSentence()
    {
    }

    internal ExampleSentence(
        Guid id,
        Guid languageId,
        string text,
        string? sourceReference,
        VocabularyVisibility visibility,
        PublicationStatus publicationStatus,
        Guid? ownerUserId,
        DateTimeOffset createdAt)
    {
        Id = id;
        LanguageId = languageId;
        Text = text;
        SourceReference = sourceReference;
        Visibility = visibility;
        PublicationStatus = publicationStatus;
        OwnerUserId = ownerUserId;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid LanguageId { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public string? SourceReference { get; private set; }

    public VocabularyVisibility Visibility { get; private set; }

    public PublicationStatus PublicationStatus { get; private set; }

    public Guid? OwnerUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
