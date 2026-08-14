namespace Spracher.Modules.Exercises.Engine;

public sealed record ExerciseDefinitionValidationResult(
    bool IsValid,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public static ExerciseDefinitionValidationResult Valid { get; } =
        new(true, new Dictionary<string, string[]>());

    public static ExerciseDefinitionValidationResult Invalid(
        string key,
        string message) =>
        new(false, new Dictionary<string, string[]> { [key] = [message] });
}

public sealed record ExerciseGradingResult(
    bool IsAccepted,
    bool IsCorrect,
    int AwardedPoints,
    int MaxPoints,
    string Feedback,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public static ExerciseGradingResult Accepted(
        bool isCorrect,
        int awardedPoints,
        int maxPoints,
        string feedback) =>
        new(
            true,
            isCorrect,
            awardedPoints,
            maxPoints,
            feedback,
            new Dictionary<string, string[]>());

    public static ExerciseGradingResult Rejected(string key, string message) =>
        new(
            false,
            false,
            0,
            0,
            string.Empty,
            new Dictionary<string, string[]> { [key] = [message] });
}

public interface IExerciseTypeHandler
{
    string TypeKey { get; }

    int SchemaVersion { get; }

    ExerciseDefinitionValidationResult ValidateDefinition(string definitionJson);

    string CreateClientPayload(string definitionJson);

    ExerciseGradingResult Grade(string definitionJson, string responseJson);
}
