using System.Text.Json;

namespace Spracher.Modules.Exercises.Engine;

public sealed class MultipleChoiceExerciseHandler : IExerciseTypeHandler
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    public string TypeKey => ExerciseTypeKeys.MultipleChoice;

    public int SchemaVersion => 1;

    public ExerciseDefinitionValidationResult ValidateDefinition(string definitionJson)
    {
        var definition = DeserializeDefinition(definitionJson);
        if (definition is null)
        {
            return ExerciseDefinitionValidationResult.Invalid(
                "definition",
                "The multiple-choice definition is not valid JSON.");
        }

        if (definition.Options is null || definition.Options.Count is < 2 or > 8)
        {
            return ExerciseDefinitionValidationResult.Invalid(
                "options",
                "A multiple-choice exercise must contain between 2 and 8 options.");
        }

        if (definition.Options.Any(option =>
                string.IsNullOrWhiteSpace(option.Id)
                || option.Id.Length > 50
                || string.IsNullOrWhiteSpace(option.Text)
                || option.Text.Length > 500))
        {
            return ExerciseDefinitionValidationResult.Invalid(
                "options",
                "Every option needs a short ID and display text.");
        }

        var optionIds = definition.Options
            .Select(option => option.Id)
            .ToHashSet(StringComparer.Ordinal);
        if (optionIds.Count != definition.Options.Count)
        {
            return ExerciseDefinitionValidationResult.Invalid(
                "options",
                "Option IDs must be unique.");
        }

        if (definition.CorrectOptionIds is null
            || definition.CorrectOptionIds.Count == 0
            || definition.CorrectOptionIds.Distinct(StringComparer.Ordinal).Count()
                != definition.CorrectOptionIds.Count
            || definition.CorrectOptionIds.Any(id => !optionIds.Contains(id)))
        {
            return ExerciseDefinitionValidationResult.Invalid(
                "correctOptionIds",
                "Correct answers must reference unique, existing option IDs.");
        }

        if (definition.Points is < 1 or > 100)
        {
            return ExerciseDefinitionValidationResult.Invalid(
                "points",
                "Points must be between 1 and 100.");
        }

        return ExerciseDefinitionValidationResult.Valid;
    }

    public string CreateClientPayload(string definitionJson)
    {
        var definition = GetValidDefinition(definitionJson);
        var payload = new MultipleChoiceClientPayload(
            definition.Options,
            definition.CorrectOptionIds.Count > 1);
        return JsonSerializer.Serialize(payload, SerializerOptions);
    }

    public ExerciseGradingResult Grade(string definitionJson, string responseJson)
    {
        var definition = GetValidDefinition(definitionJson);
        MultipleChoiceResponsePayload? response;
        try
        {
            response = JsonSerializer.Deserialize<MultipleChoiceResponsePayload>(
                responseJson,
                SerializerOptions);
        }
        catch (JsonException)
        {
            return ExerciseGradingResult.Rejected(
                "response",
                "The submitted answer is not valid JSON.");
        }

        if (response?.SelectedOptionIds is null || response.SelectedOptionIds.Count == 0)
        {
            return ExerciseGradingResult.Rejected(
                "selectedOptionIds",
                "Select at least one answer.");
        }

        var selected = response.SelectedOptionIds.ToHashSet(StringComparer.Ordinal);
        if (selected.Count != response.SelectedOptionIds.Count
            || selected.Any(id => definition.Options.All(option => option.Id != id)))
        {
            return ExerciseGradingResult.Rejected(
                "selectedOptionIds",
                "The answer contains an unknown or repeated option.");
        }

        if (definition.CorrectOptionIds.Count == 1 && selected.Count != 1)
        {
            return ExerciseGradingResult.Rejected(
                "selectedOptionIds",
                "Select exactly one answer.");
        }

        var correct = definition.CorrectOptionIds.ToHashSet(StringComparer.Ordinal);
        var isCorrect = selected.SetEquals(correct);
        return ExerciseGradingResult.Accepted(
            isCorrect,
            isCorrect ? definition.Points : 0,
            definition.Points,
            isCorrect
                ? definition.CorrectFeedback ?? "Correct answer."
                : definition.IncorrectFeedback ?? "That is not the correct answer.");
    }

    private static MultipleChoiceDefinitionPayload GetValidDefinition(string definitionJson)
    {
        var definition = DeserializeDefinition(definitionJson);
        if (definition is null)
        {
            throw new InvalidOperationException("The stored exercise definition is invalid.");
        }

        return definition;
    }

    private static MultipleChoiceDefinitionPayload? DeserializeDefinition(string definitionJson)
    {
        try
        {
            return JsonSerializer.Deserialize<MultipleChoiceDefinitionPayload>(
                definitionJson,
                SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record MultipleChoiceDefinitionPayload(
        IReadOnlyList<MultipleChoiceOptionPayload> Options,
        IReadOnlyList<string> CorrectOptionIds,
        int Points,
        string? CorrectFeedback,
        string? IncorrectFeedback);

    private sealed record MultipleChoiceClientPayload(
        IReadOnlyList<MultipleChoiceOptionPayload> Options,
        bool AllowMultiple);

    private sealed record MultipleChoiceResponsePayload(IReadOnlyList<string> SelectedOptionIds);

    private sealed record MultipleChoiceOptionPayload(string Id, string Text);
}
