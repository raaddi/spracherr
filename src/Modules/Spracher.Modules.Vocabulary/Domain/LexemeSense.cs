using Spracher.BuildingBlocks.Languages;

namespace Spracher.Modules.Vocabulary.Domain;

public sealed class LexemeSense
{
    private LexemeSense()
    {
    }

    internal LexemeSense(
        Guid id,
        Guid lexemeId,
        Guid conceptId,
        Guid definitionLanguageId,
        string definition,
        string? register,
        CefrLevel? cefrLevelOverride,
        VocabularyVisibility visibility,
        PublicationStatus publicationStatus,
        Guid? ownerUserId,
        DateTimeOffset createdAt)
    {
        Id = id;
        LexemeId = lexemeId;
        ConceptId = conceptId;
        DefinitionLanguageId = definitionLanguageId;
        Definition = definition;
        Register = register;
        CefrLevelOverride = cefrLevelOverride;
        Visibility = visibility;
        PublicationStatus = publicationStatus;
        OwnerUserId = ownerUserId;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid LexemeId { get; private set; }

    public Guid ConceptId { get; private set; }

    public Guid DefinitionLanguageId { get; private set; }

    public string Definition { get; private set; } = string.Empty;

    public string? Register { get; private set; }

    public CefrLevel? CefrLevelOverride { get; private set; }

    public VocabularyVisibility Visibility { get; private set; }

    public PublicationStatus PublicationStatus { get; private set; }

    public Guid? OwnerUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static LexemeSense CreatePrivate(
        Guid ownerUserId,
        Guid lexemeId,
        Guid conceptId,
        Guid definitionLanguageId,
        string definition,
        CefrLevel? cefrLevelOverride,
        DateTimeOffset createdAt)
    {
        if (ownerUserId == Guid.Empty)
        {
            throw new ArgumentException("Owner user ID cannot be empty.", nameof(ownerUserId));
        }

        if (lexemeId == Guid.Empty || conceptId == Guid.Empty || definitionLanguageId == Guid.Empty)
        {
            throw new ArgumentException("Vocabulary references cannot be empty.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(definition);
        return new LexemeSense(
            Guid.CreateVersion7(),
            lexemeId,
            conceptId,
            definitionLanguageId,
            definition.Trim(),
            register: null,
            cefrLevelOverride,
            VocabularyVisibility.Private,
            PublicationStatus.Draft,
            ownerUserId,
            createdAt);
    }
}
