using System.ComponentModel.DataAnnotations;

namespace Spracher.Modules.IdentityAccess.Application;

internal static class AccountRequestValidator
{
    private static readonly EmailAddressAttribute EmailValidator = new();

    public static IReadOnlyDictionary<string, string[]> ValidateRegistration(
        string? email,
        string? password,
        string? displayName,
        string? timeZoneId)
    {
        var errors = ValidateProfile(displayName, timeZoneId);

        AddEmailError(errors, email);

        if (string.IsNullOrWhiteSpace(password))
        {
            errors["password"] = ["Password is required."];
        }

        return errors;
    }

    public static Dictionary<string, string[]> ValidateProfile(
        string? displayName,
        string? timeZoneId)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        var normalizedDisplayName = displayName?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedDisplayName)
            || normalizedDisplayName.Length is < 2 or > 80)
        {
            errors["displayName"] = ["Display name must contain between 2 and 80 characters."];
        }

        if (!IsKnownTimeZone(timeZoneId))
        {
            errors["timeZoneId"] = ["The selected time zone is not supported."];
        }

        return errors;
    }

    public static bool IsValidEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email) && EmailValidator.IsValid(email.Trim());

    private static void AddEmailError(
        Dictionary<string, string[]> errors,
        string? email)
    {
        if (!IsValidEmail(email))
        {
            errors["email"] = ["A valid email address is required."];
        }
    }

    private static bool IsKnownTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId) || timeZoneId.Length > 100)
        {
            return false;
        }

        return TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out _);
    }
}
