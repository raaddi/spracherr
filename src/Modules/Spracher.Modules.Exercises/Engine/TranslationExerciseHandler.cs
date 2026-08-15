using System.Text;
using System.Text.Json;

namespace Spracher.Modules.Exercises.Engine;

public sealed class TranslationExerciseHandler : IExerciseTypeHandler
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    private static readonly char[] TerminalPunctuation = ['.', '!', '?', '…'];

    public string TypeKey => ExerciseTypeKeys.Translation;

    public int SchemaVersion => 1;

    public ExerciseDefinitionValidationResult ValidateDefinition(string definitionJson)
    {
        var definition = DeserializeDefinition(definitionJson);
        if (definition is null)
        {
            return ExerciseDefinitionValidationResult.Invalid(
                "definition",
                "The translation definition is not valid JSON.");
        }

        return ValidateParsedDefinition(definition);
    }

    private static ExerciseDefinitionValidationResult ValidateParsedDefinition(
        TranslationDefinitionPayload definition)
    {

        if (string.IsNullOrWhiteSpace(definition.SourceText)
            || definition.SourceText.Length > 1000)
        {
            return ExerciseDefinitionValidationResult.Invalid(
                "sourceText",
                "Source text is required and cannot exceed 1000 characters.");
        }

        if (!IsValidLanguageCode(definition.SourceLanguageCode)
            || !IsValidLanguageCode(definition.TargetLanguageCode)
            || string.Equals(
                definition.SourceLanguageCode,
                definition.TargetLanguageCode,
                StringComparison.OrdinalIgnoreCase))
        {
            return ExerciseDefinitionValidationResult.Invalid(
                "languageCodes",
                "Source and target language codes must be valid and different.");
        }

        if (definition.AcceptedAnswers is null
            || definition.AcceptedAnswers.Count is < 1 or > 20
            || definition.AcceptedAnswers.Any(answer =>
                string.IsNullOrWhiteSpace(answer) || answer.Length > 1000))
        {
            return ExerciseDefinitionValidationResult.Invalid(
                "acceptedAnswers",
                "Provide between 1 and 20 non-empty accepted translations.");
        }

        var normalizedAnswers = definition.AcceptedAnswers
            .Select(answer => Normalize(answer, definition))
            .ToArray();
        if (normalizedAnswers.Distinct(GetComparer(definition)).Count()
            != normalizedAnswers.Length)
        {
            return ExerciseDefinitionValidationResult.Invalid(
                "acceptedAnswers",
                "Accepted translations must be unique after normalization.");
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
        return JsonSerializer.Serialize(
            new TranslationClientPayload(
                definition.SourceText,
                definition.SourceLanguageCode,
                definition.TargetLanguageCode),
            SerializerOptions);
    }

    public ExerciseGradingResult Grade(string definitionJson, string responseJson)
    {
        var definition = GetValidDefinition(definitionJson);
        TranslationResponsePayload? response;
        try
        {
            response = JsonSerializer.Deserialize<TranslationResponsePayload>(
                responseJson,
                SerializerOptions);
        }
        catch (JsonException)
        {
            return ExerciseGradingResult.Rejected(
                "response",
                "The submitted answer is not valid JSON.");
        }

        if (string.IsNullOrWhiteSpace(response?.Answer) || response.Answer.Length > 1000)
        {
            return ExerciseGradingResult.Rejected(
                "answer",
                "Provide a translation no longer than 1000 characters.");
        }

        var submitted = Normalize(response.Answer, definition);
        var comparer = GetComparer(definition);
        var isCorrect = definition.AcceptedAnswers.Any(answer =>
            comparer.Equals(submitted, Normalize(answer, definition)));

        return ExerciseGradingResult.Accepted(
            isCorrect,
            isCorrect ? definition.Points : 0,
            definition.Points,
            isCorrect
                ? definition.CorrectFeedback ?? "Correct translation."
                : definition.IncorrectFeedback ?? "Check the translation and try again.");
    }

    private static TranslationDefinitionPayload GetValidDefinition(string definitionJson)
    {
        var definition = DeserializeDefinition(definitionJson);
        if (definition is null
            || !ValidateParsedDefinition(definition).IsValid)
        {
            throw new InvalidOperationException("The stored exercise definition is invalid.");
        }

        return definition;
    }

    private static TranslationDefinitionPayload? DeserializeDefinition(string definitionJson)
    {
        try
        {
            return JsonSerializer.Deserialize<TranslationDefinitionPayload>(
                definitionJson,
                SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool IsValidLanguageCode(string? code) =>
        !string.IsNullOrWhiteSpace(code)
        && code.Length is >= 2 and <= 12
        && code.All(character => char.IsAsciiLetter(character) || character == '-');

    private static StringComparer GetComparer(TranslationDefinitionPayload definition) =>
        definition.CaseSensitive
            ? StringComparer.Ordinal
            : StringComparer.OrdinalIgnoreCase;

    private static string Normalize(
        string value,
        TranslationDefinitionPayload definition)
    {
        var normalized = value.Normalize(NormalizationForm.FormC);
        if (definition.TrimWhitespace)
        {
            normalized = normalized.Trim();
        }

        if (definition.CollapseWhitespace)
        {
            normalized = string.Join(
                ' ',
                normalized.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        }

        if (definition.IgnoreTerminalPunctuation)
        {
            normalized = normalized.TrimEnd(TerminalPunctuation);
            if (definition.TrimWhitespace)
            {
                normalized = normalized.TrimEnd();
            }
        }

        return normalized;
    }

    private sealed record TranslationDefinitionPayload(
        string SourceText,
        string SourceLanguageCode,
        string TargetLanguageCode,
        IReadOnlyList<string> AcceptedAnswers,
        bool CaseSensitive,
        bool TrimWhitespace,
        bool CollapseWhitespace,
        bool IgnoreTerminalPunctuation,
        int Points,
        string? CorrectFeedback,
        string? IncorrectFeedback);

    private sealed record TranslationClientPayload(
        string SourceText,
        string SourceLanguageCode,
        string TargetLanguageCode);

    private sealed record TranslationResponsePayload(string Answer);
}
