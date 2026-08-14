namespace Spracher.Modules.Vocabulary.Application;

internal enum VocabularyResultKind
{
    Success = 0,
    ValidationError = 1,
    NotFound = 2,
    Conflict = 3,
}

internal sealed record VocabularyResult<T>(
    VocabularyResultKind Kind,
    T? Value,
    IReadOnlyDictionary<string, string[]> Errors)
{
    private static readonly IReadOnlyDictionary<string, string[]> NoErrors =
        new Dictionary<string, string[]>();

    public static VocabularyResult<T> Success(T value) =>
        new(VocabularyResultKind.Success, value, NoErrors);

    public static VocabularyResult<T> Validation(string key, string message) =>
        new(
            VocabularyResultKind.ValidationError,
            default,
            new Dictionary<string, string[]> { [key] = [message] });

    public static VocabularyResult<T> NotFound() =>
        new(VocabularyResultKind.NotFound, default, NoErrors);

    public static VocabularyResult<T> Conflict(string key, string message) =>
        new(
            VocabularyResultKind.Conflict,
            default,
            new Dictionary<string, string[]> { [key] = [message] });
}
