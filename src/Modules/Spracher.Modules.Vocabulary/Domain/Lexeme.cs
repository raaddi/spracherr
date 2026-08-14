using Spracher.BuildingBlocks.Languages;

namespace Spracher.Modules.Vocabulary.Domain;

public sealed class Lexeme
{
    private Lexeme()
    {
    }

    internal Lexeme(
        Guid id,
        Guid languageId,
        string lemma,
        PartOfSpeech partOfSpeech,
        CefrLevel? cefrLevel,
        int? frequencyRank,
        string? notes,
        VocabularyVisibility visibility,
        VocabularySourceType sourceType,
        string? sourceReference,
        PublicationStatus publicationStatus,
        Guid? ownerUserId,
        DateTimeOffset createdAt)
    {
        Id = id;
        LanguageId = languageId;
        Lemma = lemma;
        NormalizedLemma = VocabularyTextNormalizer.NormalizeLemma(lemma);
        PartOfSpeech = partOfSpeech;
        CefrLevel = cefrLevel;
        FrequencyRank = frequencyRank;
        Notes = notes;
        Visibility = visibility;
        SourceType = sourceType;
        SourceReference = sourceReference;
        PublicationStatus = publicationStatus;
        OwnerUserId = ownerUserId;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid LanguageId { get; private set; }

    public string Lemma { get; private set; } = string.Empty;

    public string NormalizedLemma { get; private set; } = string.Empty;

    public PartOfSpeech PartOfSpeech { get; private set; }

    public CefrLevel? CefrLevel { get; private set; }

    public int? FrequencyRank { get; private set; }

    public string? Notes { get; private set; }

    public VocabularyVisibility Visibility { get; private set; }

    public VocabularySourceType SourceType { get; private set; }

    public string? SourceReference { get; private set; }

    public PublicationStatus PublicationStatus { get; private set; }

    public Guid? OwnerUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static Lexeme CreatePrivate(
        Guid ownerUserId,
        Guid languageId,
        string lemma,
        PartOfSpeech partOfSpeech,
        CefrLevel? cefrLevel,
        string? notes,
        DateTimeOffset createdAt)
    {
        if (ownerUserId == Guid.Empty)
        {
            throw new ArgumentException("Owner user ID cannot be empty.", nameof(ownerUserId));
        }

        if (languageId == Guid.Empty)
        {
            throw new ArgumentException("Language ID cannot be empty.", nameof(languageId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(lemma);
        return new Lexeme(
            Guid.CreateVersion7(),
            languageId,
            lemma.Trim(),
            partOfSpeech,
            cefrLevel,
            frequencyRank: null,
            notes?.Trim(),
            VocabularyVisibility.Private,
            VocabularySourceType.UserCreated,
            sourceReference: null,
            PublicationStatus.Draft,
            ownerUserId,
            createdAt);
    }
}
