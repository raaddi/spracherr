namespace Spracher.Contracts.Identity;

public sealed record AuthSessionResponse(
    bool IsAuthenticated,
    AuthenticatedUserResponse? User);

public sealed record AuthenticatedUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string TimeZoneId,
    IReadOnlyList<string> Roles);
