namespace Spracher.Contracts.Identity;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string DisplayName,
    string TimeZoneId);
