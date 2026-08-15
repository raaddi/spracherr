using System.Text.Json;

namespace Spracher.Modules.Exercises.Engine;

public sealed class FillInBlankExerciseHandler : IExerciseTypeHandler
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    public string TypeKey => ExerciseTypeKeys.FillInBlank;

    public int SchemaVersion => 1;

    public ExerciseDefinitionValidationResult ValidateDefinition(string definitionJson)
    {
        var definition = DeserializeDefinition(definitionJson);
        if (definition is null)
        {
            return ExerciseDefinitionValidationResult.Invalid(
                "definition",
                "The fill-in-blank definition is not valid JSON.");
        }

        return ValidateParsedDefinition(definition);
    }

    public string CreateClientPayload(string definitionJson)
    {
        var definition = GetValidDefinition(definitionJson);
        return JsonSerializer.Serialize(
            new FillInBlankClientPayload(definition.Segments),
            SerializerOptions);
    }

    public ExerciseGradingResult Grade(string definitionJson, string responseJson)
    {
        var definition = GetValidDefinition(definitionJson);
        FillInBlankResponsePayload? response;
        try
        {
            response = JsonSerializer.Deserialize<FillInBlankResponsePayload>(
                responseJson,
                SerializerOptions);
        }
        catch (JsonException)
        {
            return ExerciseGradingResult.Rejected(
                "response",
                "The submitted answer is not valid JSON.");
        }

        var blankIds = definition.Segments
            .Where(segment => segment.Kind == SegmentKinds.Blank)
            .Select(segment => segment.BlankId!)
            .ToHashSet(StringComparer.Ordinal);
        if (response?.Answers is null
            || response.Answers.Count != blankIds.Count
            || response.Answers.Keys.Any(key => !blankIds.Contains(key))
            || response.Answers.Values.Any(string.IsNullOrWhiteSpace))
        {
            return ExerciseGradingResult.Rejected(
                "answers",
                "Provide one answer for every blank.");
        }

        var comparison = definition.CaseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;
        var isCorrect = definition.Answers.All(pair =>
        {
            var submitted = Normalize(response.Answers[pair.Key], definition.TrimWhitespace);
            return pair.Value.Any(accepted => string.Equals(
                submitted,
                Normalize(accepted, definition.TrimWhitespace),
                comparison));
        });

        return ExerciseGradingResult.Accepted(
            isCorrect,
            isCorrect ? definition.Points : 0,
            definition.Points,
            isCorrect
                ? definition.CorrectFeedback ?? "Correct answer."
                : definition.IncorrectFeedback ?? "Check the missing word and try again.");
    }

    private static ExerciseDefinitionValidationResult ValidateParsedDefinition(
        FillInBlankDefinitionPayload definition)
    {
        if (definition.Segments is null || definition.Segments.Count is < 2 or > 25)
        {
            return ExerciseDefinitionValidationResult.Invalid(
                "segments",
                "A fill-in-blank exercise must contain between 2 and 25 segments.");
        }

        if (definition.Segments.Any(segment => !IsValidSegment(segment)))
        {
            return ExerciseDefinitionValidationResult.Invalid(
                "segments",
                "Every segment must be valid text or a blank with a short ID.");
        }

        var blankIds = definition.Segments
            .Where(segment => segment.Kind == SegmentKinds.Blank)
            .Select(segment => segment.BlankId!)
            .ToArray();
        if (blankIds.Length is < 1 or > 5
            || blankIds.Distinct(StringComparer.Ordinal).Count() != blankIds.Length)
        {
            return ExerciseDefinitionValidationResult.Invalid(
                "segments",
                "The exercise needs between 1 and 5 blanks with unique IDs.");
        }

        var expectedBlankIds = blankIds.ToHashSet(StringComparer.Ordinal);
        if (definition.Answers is null
            || definition.Answers.Count != expectedBlankIds.Count
            || definition.Answers.Keys.Any(key => !expectedBlankIds.Contains(key))
            || definition.Answers.Values.Any(variants =>
                variants is null
                || variants.Count is < 1 or > 10
                || variants.Any(answer =>
                    string.IsNullOrWhiteSpace(answer) || answer.Length > 200)))
        {
            return ExerciseDefinitionValidationResult.Invalid(
                "answers",
                "Every blank needs between 1 and 10 accepted answers.");
        }

        if (definition.Points is < 1 or > 100)
        {
            return ExerciseDefinitionValidationResult.Invalid(
                "points",
                "Points must be between 1 and 100.");
        }

        return ExerciseDefinitionValidationResult.Valid;
    }

    private static bool IsValidSegment(FillInBlankSegmentPayload segment) =>
        segment.Kind switch
        {
            SegmentKinds.Text => segment.Text is not null
                                 && segment.Text.Length <= 500
                                 && string.IsNullOrEmpty(segment.BlankId),
            SegmentKinds.Blank => !string.IsNullOrWhiteSpace(segment.BlankId)
                                  && segment.BlankId.Length <= 50
                                  && string.IsNullOrEmpty(segment.Text),
            _ => false,
        };

    private static FillInBlankDefinitionPayload GetValidDefinition(string definitionJson)
    {
        var definition = DeserializeDefinition(definitionJson);
        if (definition is null || !ValidateParsedDefinition(definition).IsValid)
        {
            throw new InvalidOperationException("The stored exercise definition is invalid.");
        }

        return definition;
    }

    private static FillInBlankDefinitionPayload? DeserializeDefinition(string definitionJson)
    {
        try
        {
            return JsonSerializer.Deserialize<FillInBlankDefinitionPayload>(
                definitionJson,
                SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string Normalize(string value, bool trimWhitespace) =>
        trimWhitespace ? value.Trim() : value;

    private static class SegmentKinds
    {
        public const string Text = "text";
        public const string Blank = "blank";
    }

    private sealed record FillInBlankDefinitionPayload(
        IReadOnlyList<FillInBlankSegmentPayload> Segments,
        IReadOnlyDictionary<string, IReadOnlyList<string>> Answers,
        bool CaseSensitive,
        bool TrimWhitespace,
        int Points,
        string? CorrectFeedback,
        string? IncorrectFeedback);

    private sealed record FillInBlankClientPayload(
        IReadOnlyList<FillInBlankSegmentPayload> Segments);

    private sealed record FillInBlankResponsePayload(
        IReadOnlyDictionary<string, string> Answers);

    private sealed record FillInBlankSegmentPayload(
        string Kind,
        string? Text,
        string? BlankId);
}
