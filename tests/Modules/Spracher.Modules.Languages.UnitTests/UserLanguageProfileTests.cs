using Spracher.BuildingBlocks.Languages;
using Spracher.Modules.Languages.Domain;

namespace Spracher.Modules.Languages.UnitTests;

public sealed class UserLanguageProfileTests
{
    private static readonly Guid UserId = Guid.Parse("0198ad00-0000-7000-8000-000000000001");
    private static readonly Guid LanguageId = Guid.Parse("0198ad00-0000-7000-8000-000000000002");

    [Fact]
    public void NewLearningLanguageShouldStartAtA0()
    {
        var startedAt = new DateTimeOffset(2026, 8, 14, 17, 0, 0, TimeSpan.Zero);

        var profile = UserLanguageProfile.Create(
            UserId,
            LanguageId,
            isNative: false,
            isLearning: true,
            startedAt);

        Assert.True(profile.IsLearning);
        Assert.Equal(CefrLevel.A0, profile.CurrentCefrLevel);
        Assert.Equal(startedAt, profile.StartedAt);
    }

    [Fact]
    public void StoppingLearningShouldClearLearningState()
    {
        var startedAt = new DateTimeOffset(2026, 8, 14, 17, 0, 0, TimeSpan.Zero);
        var updatedAt = startedAt.AddDays(1);
        var profile = UserLanguageProfile.Create(
            UserId,
            LanguageId,
            isNative: true,
            isLearning: true,
            startedAt);

        profile.UpdateSelection(isNative: true, isLearning: false, updatedAt);

        Assert.True(profile.IsNative);
        Assert.False(profile.IsLearning);
        Assert.Null(profile.CurrentCefrLevel);
        Assert.Null(profile.StartedAt);
        Assert.Equal(updatedAt, profile.UpdatedAt);
    }

    [Fact]
    public void UnselectedLanguageShouldBeRejected()
    {
        var createdAt = new DateTimeOffset(2026, 8, 14, 17, 0, 0, TimeSpan.Zero);

        var exception = Assert.Throws<ArgumentException>(() =>
            UserLanguageProfile.Create(
                UserId,
                LanguageId,
                isNative: false,
                isLearning: false,
                createdAt));

        Assert.Contains("native, learning, or both", exception.Message, StringComparison.Ordinal);
    }
}
