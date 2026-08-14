namespace Spracher.Modules.Vocabulary.Domain;

public sealed class ExampleUsage
{
    private ExampleUsage()
    {
    }

    internal ExampleUsage(
        Guid lexemeSenseId,
        Guid exampleSentenceId,
        int? highlightStart,
        int? highlightLength)
    {
        LexemeSenseId = lexemeSenseId;
        ExampleSentenceId = exampleSentenceId;
        HighlightStart = highlightStart;
        HighlightLength = highlightLength;
    }

    public Guid LexemeSenseId { get; private set; }

    public Guid ExampleSentenceId { get; private set; }

    public int? HighlightStart { get; private set; }

    public int? HighlightLength { get; private set; }
}
