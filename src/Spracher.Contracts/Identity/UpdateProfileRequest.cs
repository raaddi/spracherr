namespace Spracher.Contracts.Identity;

public sealed record UpdateProfileRequest(
    string DisplayName,
    string TimeZoneId);
