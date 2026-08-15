using System.Text.Json;
using Spracher.Modules.Exercises.Engine;

namespace Spracher.Modules.Exercises.UnitTests;

public sealed class FillInBlankExerciseHandlerTests
{
    private const string DefinitionJson = """
        {
          "segments": [
            { "kind": "text", "text": "She ", "blankId": null },
            { "kind": "blank", "text": null, "blankId": "verb" },
            { "kind": "text", "text": " ready.", "blankId": null }
          ],
          "answers": { "verb": ["is", "'s"] },
          "caseSensitive": false,
          "trimWhitespace": true,
          "points": 8,
          "correctFeedback": "Correct.",
          "incorrectFeedback": "Try again."
        }
        """;

    private readonly FillInBlankExerciseHandler _handler = new();

    [Fact]
    public void ClientPayloadShouldContainSegmentsWithoutAnswersOrFeedback()
    {
        var payload = _handler.CreateClientPayload(DefinitionJson);
        using var document = JsonDocument.Parse(payload);

        Assert.Equal(3, document.RootElement.GetProperty("segments").GetArrayLength());
        Assert.False(document.RootElement.TryGetProperty("answers", out _));
        Assert.False(document.RootElement.TryGetProperty("correctFeedback", out _));
        Assert.False(document.RootElement.TryGetProperty("incorrectFeedback", out _));
    }

    [Theory]
    [InlineData("is")]
    [InlineData(" IS ")]
    [InlineData("'s")]
    public void GradeShouldAcceptConfiguredVariants(string answer)
    {
        var response = $$"""{ "answers": { "verb": "{{answer}}" } }""";

        var result = _handler.Grade(DefinitionJson, response);

        Assert.True(result.IsAccepted);
        Assert.True(result.IsCorrect);
        Assert.Equal(8, result.AwardedPoints);
    }

    [Fact]
    public void GradeShouldRejectMissingBlank()
    {
        var result = _handler.Grade(DefinitionJson, """{ "answers": {} }""");

        Assert.False(result.IsAccepted);
        Assert.Contains("answers", result.Errors.Keys);
    }

    [Fact]
    public void DefinitionShouldRequireAnswersForEveryBlank()
    {
        const string invalidDefinition = """
            {
              "segments": [
                { "kind": "blank", "text": null, "blankId": "first" },
                { "kind": "blank", "text": null, "blankId": "second" }
              ],
              "answers": { "first": ["one"] },
              "caseSensitive": false,
              "trimWhitespace": true,
              "points": 5
            }
            """;

        var result = _handler.ValidateDefinition(invalidDefinition);

        Assert.False(result.IsValid);
        Assert.Contains("answers", result.Errors.Keys);
    }
}
