using Spracher.IdentityModel;

namespace Spracher.IdentityModel.UnitTests;

public sealed class ApplicationUserTests
{
    [Fact]
    public void CreateShouldNormalizeOwnedProfileValues()
    {
        var createdAt = new DateTimeOffset(2026, 8, 14, 17, 0, 0, TimeSpan.Zero);

        var user = ApplicationUser.Create(
            " learner@example.com ",
            " Ada ",
            "Europe/Warsaw",
            createdAt);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("learner@example.com", user.Email);
        Assert.Equal("learner@example.com", user.UserName);
        Assert.Equal("Ada", user.DisplayName);
        Assert.Equal(AccountStatus.Active, user.Status);
        Assert.Equal(createdAt, user.CreatedAt);
    }

    [Fact]
    public void UpdateProfileShouldTrackChangeTime()
    {
        var createdAt = new DateTimeOffset(2026, 8, 14, 17, 0, 0, TimeSpan.Zero);
        var updatedAt = createdAt.AddHours(2);
        var user = ApplicationUser.Create(
            "learner@example.com",
            "Ada",
            "UTC",
            createdAt);

        user.UpdateProfile(" Ada Lovelace ", "Europe/Warsaw", updatedAt);

        Assert.Equal("Ada Lovelace", user.DisplayName);
        Assert.Equal("Europe/Warsaw", user.TimeZoneId);
        Assert.Equal(updatedAt, user.ProfileUpdatedAt);
    }
}
