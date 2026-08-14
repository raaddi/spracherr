namespace Spracher.Modules.Vocabulary.Domain;

public sealed class LexemeFeature
{
    private LexemeFeature()
    {
    }

    internal LexemeFeature(Guid id, Guid lexemeId, string key, string value)
    {
        Id = id;
        LexemeId = lexemeId;
        Key = key;
        Value = value;
    }

    public Guid Id { get; private set; }

    public Guid LexemeId { get; private set; }

    public string Key { get; private set; } = string.Empty;

    public string Value { get; private set; } = string.Empty;
}
