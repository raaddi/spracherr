namespace Spracher.Modules.Exercises.Application;

internal enum ExerciseResultKind
{
    Success = 0,
    ValidationError = 1,
    NotFound = 2,
    Conflict = 3,
}

internal sealed record ExerciseResult<T>(
    ExerciseResultKind Kind,
    T? Value,
    IReadOnlyDictionary<string, string[]> Errors)
{
    private static readonly IReadOnlyDictionary<string, string[]> NoErrors =
        new Dictionary<string, string[]>();

    public static ExerciseResult<T> Success(T value) =>
        new(ExerciseResultKind.Success, value, NoErrors);

    public static ExerciseResult<T> Validation(
        IReadOnlyDictionary<string, string[]> errors) =>
        new(ExerciseResultKind.ValidationError, default, errors);

    public static ExerciseResult<T> Validation(string key, string message) =>
        Validation(new Dictionary<string, string[]> { [key] = [message] });

    public static ExerciseResult<T> NotFound() =>
        new(ExerciseResultKind.NotFound, default, NoErrors);

    public static ExerciseResult<T> Conflict(string key, string message) =>
        new(
            ExerciseResultKind.Conflict,
            default,
            new Dictionary<string, string[]> { [key] = [message] });
}
