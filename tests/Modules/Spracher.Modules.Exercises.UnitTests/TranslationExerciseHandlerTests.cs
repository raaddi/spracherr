using System.Text.Json;
using Spracher.Modules.Exercises.Engine;

namespace Spracher.Modules.Exercises.UnitTests;

public sealed class TranslationExerciseHandlerTests
{
    private const string DefinitionJson = """
        {
          "sourceText": "She goes to school every day.",
          "sourceLanguageCode": "en",
          "targetLanguageCode": "pl",
          "acceptedAnswers": [
            "Ona chodzi do szkoły codziennie.",
            "Ona codziennie chodzi do szkoły."
          ],
          "caseSensitive": false,
          "trimWhitespace": true,
          "collapseWhitespace": true,
          "ignoreTerminalPunctuation": true,
          "points": 12,
          "correctFeedback": "Correct.",
          "incorrectFeedback": "Try again."
        }
        """;

    private readonly TranslationExerciseHandler _handler = new();

    [Fact]
    public void ClientPayloadShouldNotExposeAcceptedAnswersOrFeedback()
    {
        var payload = _handler.CreateClientPayload(DefinitionJson);
        using var document = JsonDocument.Parse(payload);

        Assert.Equal(
            "She goes to school every day.",
            document.RootElement.GetProperty("sourceText").GetString());
        Assert.Equal("en", document.RootElement.GetProperty("sourceLanguageCode").GetString());
        Assert.Equal("pl", document.RootElement.GetProperty("targetLanguageCode").GetString());
        Assert.False(document.RootElement.TryGetProperty("acceptedAnswers", out _));
        Assert.False(document.RootElement.TryGetProperty("correctFeedback", out _));
    }

    [Theory]
    [InlineData("ona chodzi do szkoły codziennie")]
    [InlineData("  Ona   codziennie chodzi do szkoły!  ")]
    public void GradeShouldAcceptConfiguredNormalizedVariants(string answer)
    {
        var response = JsonSerializer.Serialize(new { answer });

        var result = _handler.Grade(DefinitionJson, response);

        Assert.True(result.IsAccepted);
        Assert.True(result.IsCorrect);
        Assert.Equal(12, result.AwardedPoints);
    }

    [Fact]
    public void GradeShouldRejectEmptyAnswer()
    {
        var result = _handler.Grade(DefinitionJson, """{ "answer": " " }""");

        Assert.False(result.IsAccepted);
        Assert.Contains("answer", result.Errors.Keys);
    }

    [Fact]
    public void DefinitionShouldRejectEquivalentDuplicateAnswers()
    {
        const string invalidDefinition = """
            {
              "sourceText": "Good morning.",
              "sourceLanguageCode": "en",
              "targetLanguageCode": "pl",
              "acceptedAnswers": ["Dzień dobry.", "dzień dobry"],
              "caseSensitive": false,
              "trimWhitespace": true,
              "collapseWhitespace": true,
              "ignoreTerminalPunctuation": true,
              "points": 5
            }
            """;

        var result = _handler.ValidateDefinition(invalidDefinition);

        Assert.False(result.IsValid);
        Assert.Contains("acceptedAnswers", result.Errors.Keys);
    }
}
