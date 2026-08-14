using System.Globalization;
using System.Text;

namespace Spracher.Modules.Vocabulary.Domain;

public static class VocabularyTextNormalizer
{
    public static string NormalizeLemma(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().Normalize(NormalizationForm.FormKC).ToLowerInvariant();
        var result = new StringBuilder(normalized.Length);
        var previousWasWhitespace = false;

        foreach (var character in normalized)
        {
            var isWhitespace = char.GetUnicodeCategory(character) is UnicodeCategory.SpaceSeparator
                or UnicodeCategory.LineSeparator
                or UnicodeCategory.ParagraphSeparator
                || char.IsWhiteSpace(character);
            if (isWhitespace)
            {
                if (!previousWasWhitespace)
                {
                    result.Append(' ');
                    previousWasWhitespace = true;
                }

                continue;
            }

            result.Append(character);
            previousWasWhitespace = false;
        }

        return result.ToString();
    }
}
