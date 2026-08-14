namespace Spracher.Modules.Exercises.Domain;

public sealed class ExerciseAttempt
{
    private ExerciseAttempt()
    {
    }

    private ExerciseAttempt(Guid userId, Guid exerciseVersionId, DateTimeOffset startedAt)
    {
        Id = Guid.CreateVersion7();
        UserId = userId;
        ExerciseVersionId = exerciseVersionId;
        Status = ExerciseAttemptStatus.InProgress;
        StartedAt = startedAt;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid ExerciseVersionId { get; private set; }

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

        return new ExerciseAttempt(userId, exerciseVersionId, startedAt);
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
