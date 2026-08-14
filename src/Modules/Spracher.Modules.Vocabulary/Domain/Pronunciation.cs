namespace Spracher.Modules.Vocabulary.Domain;

public sealed class Pronunciation
{
    private Pronunciation()
    {
    }

    internal Pronunciation(
        Guid id,
        Guid lexemeId,
        string scheme,
        string value,
        string? region,
        string? audioAssetReference)
    {
        Id = id;
        LexemeId = lexemeId;
        Scheme = scheme;
        Value = value;
        Region = region;
        AudioAssetReference = audioAssetReference;
    }

    public Guid Id { get; private set; }

    public Guid LexemeId { get; private set; }

    public string Scheme { get; private set; } = string.Empty;

    public string Value { get; private set; } = string.Empty;

    public string? Region { get; private set; }

    public string? AudioAssetReference { get; private set; }
}
