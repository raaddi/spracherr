using System.Text.Json;
using Spracher.Modules.Exercises.Engine;

namespace Spracher.Modules.Exercises.UnitTests;

public sealed class MultipleChoiceExerciseHandlerTests
{
    private const string DefinitionJson = """
        {
          "options": [
            { "id": "a", "text": "First" },
            { "id": "b", "text": "Second" }
          ],
          "correctOptionIds": ["b"],
          "points": 10,
          "correctFeedback": "Correct.",
          "incorrectFeedback": "Try again."
        }
        """;

    private readonly MultipleChoiceExerciseHandler _handler = new();

    [Fact]
    public void ClientPayloadShouldNotExposeCorrectAnswerOrFeedback()
    {
        var payload = _handler.CreateClientPayload(DefinitionJson);
        using var document = JsonDocument.Parse(payload);

        Assert.True(document.RootElement.TryGetProperty("options", out var options));
        Assert.Equal(2, options.GetArrayLength());
        Assert.False(document.RootElement.TryGetProperty("correctOptionIds", out _));
        Assert.False(document.RootElement.TryGetProperty("correctFeedback", out _));
        Assert.False(document.RootElement.TryGetProperty("incorrectFeedback", out _));
    }

    [Theory]
    [InlineData("b", true, 10)]
    [InlineData("a", false, 0)]
    public void GradeShouldBeDeterministicAndServerControlled(
        string selectedOptionId,
        bool expectedCorrect,
        int expectedPoints)
    {
        var response = $$"""
            { "selectedOptionIds": ["{{selectedOptionId}}"] }
            """;

        var first = _handler.Grade(DefinitionJson, response);
        var second = _handler.Grade(DefinitionJson, response);

        Assert.True(first.IsAccepted);
        Assert.Equal(expectedCorrect, first.IsCorrect);
        Assert.Equal(expectedPoints, first.AwardedPoints);
        Assert.Equal(first.IsAccepted, second.IsAccepted);
        Assert.Equal(first.IsCorrect, second.IsCorrect);
        Assert.Equal(first.AwardedPoints, second.AwardedPoints);
        Assert.Equal(first.MaxPoints, second.MaxPoints);
        Assert.Equal(first.Feedback, second.Feedback);
    }

    [Fact]
    public void GradeShouldRejectUnknownOption()
    {
        var result = _handler.Grade(
            DefinitionJson,
            """{ "selectedOptionIds": ["forged"], "awardedPoints": 999 }""");

        Assert.False(result.IsAccepted);
        Assert.Contains("selectedOptionIds", result.Errors.Keys);
    }
}
