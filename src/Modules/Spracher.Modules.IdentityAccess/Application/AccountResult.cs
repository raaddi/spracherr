namespace Spracher.Modules.IdentityAccess.Application;

internal enum AccountResultKind
{
    Success,
    Accepted,
    ValidationError,
    Conflict,
    Unauthorized,
    LockedOut,
    NotAllowed,
}

internal sealed record AccountResult<T>(
    AccountResultKind Kind,
    T? Value,
    IReadOnlyDictionary<string, string[]> Errors)
{
    private static readonly IReadOnlyDictionary<string, string[]> NoErrors =
        new Dictionary<string, string[]>();

    public static AccountResult<T> Success(T value) =>
        new(AccountResultKind.Success, value, NoErrors);

    public static AccountResult<T> Accepted(T value) =>
        new(AccountResultKind.Accepted, value, NoErrors);

    public static AccountResult<T> Validation(
        IReadOnlyDictionary<string, string[]> errors) =>
        new(AccountResultKind.ValidationError, default, errors);

    public static AccountResult<T> Conflict(string code, string message) =>
        new(
            AccountResultKind.Conflict,
            default,
            new Dictionary<string, string[]> { [code] = [message] });

    public static AccountResult<T> Unauthorized() =>
        new(AccountResultKind.Unauthorized, default, NoErrors);

    public static AccountResult<T> LockedOut() =>
        new(AccountResultKind.LockedOut, default, NoErrors);

    public static AccountResult<T> NotAllowed() =>
        new(AccountResultKind.NotAllowed, default, NoErrors);
}
