namespace Spracher.Modules.Vocabulary.Domain;

public sealed class WordForm
{
    private WordForm()
    {
    }

    internal WordForm(Guid id, Guid lexemeId, string form, string grammarTags)
    {
        Id = id;
        LexemeId = lexemeId;
        Form = form;
        NormalizedForm = VocabularyTextNormalizer.NormalizeLemma(form);
        GrammarTags = grammarTags;
    }

    public Guid Id { get; private set; }

    public Guid LexemeId { get; private set; }

    public string Form { get; private set; } = string.Empty;

    public string NormalizedForm { get; private set; } = string.Empty;

    public string GrammarTags { get; private set; } = string.Empty;
}
