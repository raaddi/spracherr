using Spracher.Modules.Vocabulary.Domain;

namespace Spracher.Modules.Vocabulary.UnitTests;

public sealed class VocabularyTextNormalizerTests
{
    [Theory]
    [InlineData("  River   Bank  ", "river bank")]
    [InlineData("JABŁKO", "jabłko")]
    [InlineData("full\u3000width", "full width")]
    public void NormalizeLemmaShouldProduceStableSearchValue(
        string value,
        string expected)
    {
        var normalized = VocabularyTextNormalizer.NormalizeLemma(value);

        Assert.Equal(expected, normalized);
    }
}
