using Spracher.Modules.Exercises.Domain;

namespace Spracher.Modules.Exercises.UnitTests;

public sealed class ExerciseDomainTests
{
    [Fact]
    public void PublishedVersionShouldNotBePublishedAgain()
    {
        var now = new DateTimeOffset(2026, 8, 14, 12, 0, 0, TimeSpan.Zero);
        var definition = ExerciseDefinition.Create(
            "multiple-choice",
            "A title",
            description: null,
            now);
        var version = ExerciseVersion.CreateDraft(
            definition.Id,
            versionNumber: 1,
            schemaVersion: 1,
            "Choose an answer.",
            "{}",
            now);

        version.Publish(now);

        Assert.Equal(ExerciseVersionStatus.Published, version.Status);
        Assert.Equal(now, version.PublishedAt);
        Assert.Throws<InvalidOperationException>(() => version.Publish(now.AddMinutes(1)));
    }

    [Fact]
    public void CompletedAttemptShouldStoreServerScoreAndRejectSecondCompletion()
    {
        var now = new DateTimeOffset(2026, 8, 14, 12, 0, 0, TimeSpan.Zero);
        var attempt = ExerciseAttempt.Start(Guid.NewGuid(), Guid.NewGuid(), now);

        attempt.Complete(10, 10, now.AddSeconds(5));

        Assert.Equal(ExerciseAttemptStatus.Completed, attempt.Status);
        Assert.Equal(10, attempt.AwardedPoints);
        Assert.Throws<InvalidOperationException>(
            () => attempt.Complete(0, 10, now.AddSeconds(10)));
    }

    [Fact]
    public void OwnedDefinitionShouldRetainAuthorIdentity()
    {
        var ownerUserId = Guid.NewGuid();

        var definition = ExerciseDefinition.CreateOwned(
            ownerUserId,
            "fill-in-blank",
            "Owned exercise",
            description: null,
            DateTimeOffset.UtcNow);

        Assert.Equal(ownerUserId, definition.OwnerUserId);
    }

    [Fact]
    public void PublishedSetShouldRetainPinnedOrderAndRejectSecondPublication()
    {
        var now = new DateTimeOffset(2026, 8, 15, 12, 0, 0, TimeSpan.Zero);
        var set = ExerciseSet.CreateDraft(
            Guid.NewGuid(),
            "Quick practice",
            description: null,
            now);
        var first = ExerciseSetItem.Create(set.Id, Guid.NewGuid(), position: 1);
        var second = ExerciseSetItem.Create(set.Id, Guid.NewGuid(), position: 2);

        set.Publish(now);

        Assert.Equal(ExerciseSetStatus.Published, set.Status);
        Assert.Equal(1, first.Position);
        Assert.Equal(2, second.Position);
        Assert.Throws<InvalidOperationException>(() => set.Publish(now.AddMinutes(1)));
    }

    [Fact]
    public void AttemptStartedFromSetShouldRetainSourceItem()
    {
        var setItemId = Guid.NewGuid();

        var attempt = ExerciseAttempt.StartFromSet(
            Guid.NewGuid(),
            Guid.NewGuid(),
            setItemId,
            DateTimeOffset.UtcNow);

        Assert.Equal(setItemId, attempt.ExerciseSetItemId);
    }
}
