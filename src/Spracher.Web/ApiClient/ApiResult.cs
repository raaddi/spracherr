namespace Spracher.Web.ApiClient;

public sealed record ApiResult<T>(
    bool Succeeded,
    T? Value,
    string? ErrorMessage,
    IReadOnlyDictionary<string, string[]> Errors);

public static class ApiResult
{
    private static readonly IReadOnlyDictionary<string, string[]> NoErrors =
        new Dictionary<string, string[]>();

    public static ApiResult<T> Success<T>(T value) =>
        new(true, value, null, NoErrors);

    public static ApiResult<T> Failure<T>(
        string errorMessage,
        IReadOnlyDictionary<string, string[]>? errors = null) =>
        new(false, default, errorMessage, errors ?? NoErrors);
}
