namespace Spracher.Modules.Exercises.Domain;

public sealed class ExerciseAttempt
{
    private ExerciseAttempt()
    {
    }

    private ExerciseAttempt(
        Guid userId,
        Guid exerciseVersionId,
        Guid? exerciseSetItemId,
        DateTimeOffset startedAt)
    {
        Id = Guid.CreateVersion7();
        UserId = userId;
        ExerciseVersionId = exerciseVersionId;
        ExerciseSetItemId = exerciseSetItemId;
        Status = ExerciseAttemptStatus.InProgress;
        StartedAt = startedAt;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid ExerciseVersionId { get; private set; }

    public Guid? ExerciseSetItemId { get; private set; }

    public ExerciseAttemptStatus Status { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public int? AwardedPoints { get; private set; }

    public int? MaxPoints { get; private set; }

    public static ExerciseAttempt Start(
        Guid userId,
        Guid exerciseVersionId,
        DateTimeOffset startedAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        if (exerciseVersionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Exercise version ID cannot be empty.",
                nameof(exerciseVersionId));
        }

        return new ExerciseAttempt(userId, exerciseVersionId, exerciseSetItemId: null, startedAt);
    }

    public static ExerciseAttempt StartFromSet(
        Guid userId,
        Guid exerciseVersionId,
        Guid exerciseSetItemId,
        DateTimeOffset startedAt)
    {
        if (exerciseSetItemId == Guid.Empty)
        {
            throw new ArgumentException(
                "Exercise set item ID cannot be empty.",
                nameof(exerciseSetItemId));
        }

        var attempt = Start(userId, exerciseVersionId, startedAt);
        attempt.ExerciseSetItemId = exerciseSetItemId;
        return attempt;
    }

    public void Complete(int awardedPoints, int maxPoints, DateTimeOffset completedAt)
    {
        if (Status != ExerciseAttemptStatus.InProgress)
        {
            throw new InvalidOperationException("The exercise attempt is already completed.");
        }

        if (maxPoints < 1 || awardedPoints < 0 || awardedPoints > maxPoints)
        {
            throw new ArgumentOutOfRangeException(
                nameof(awardedPoints),
                "Awarded points must be between zero and the maximum score.");
        }

        Status = ExerciseAttemptStatus.Completed;
        AwardedPoints = awardedPoints;
        MaxPoints = maxPoints;
        CompletedAt = completedAt;
    }
}
