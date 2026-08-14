using Microsoft.AspNetCore.Identity;

namespace Spracher.IdentityModel;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    private ApplicationUser()
    {
    }

    private ApplicationUser(
        Guid id,
        string email,
        string displayName,
        string timeZoneId,
        DateTimeOffset createdAt)
    {
        Id = id;
        Email = email;
        UserName = email;
        DisplayName = displayName;
        TimeZoneId = timeZoneId;
        CreatedAt = createdAt;
        Status = AccountStatus.Active;
        SecurityStamp = Guid.NewGuid().ToString("N");
    }

    public string DisplayName { get; private set; } = string.Empty;

    public string TimeZoneId { get; private set; } = "UTC";

    public AccountStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ProfileUpdatedAt { get; private set; }

    public static ApplicationUser Create(
        string email,
        string displayName,
        string timeZoneId,
        DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);

        return new ApplicationUser(
            Guid.CreateVersion7(),
            email.Trim(),
            displayName.Trim(),
            timeZoneId,
            createdAt);
    }

    public void UpdateProfile(
        string displayName,
        string timeZoneId,
        DateTimeOffset updatedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);

        DisplayName = displayName.Trim();
        TimeZoneId = timeZoneId;
        ProfileUpdatedAt = updatedAt;
    }
}
