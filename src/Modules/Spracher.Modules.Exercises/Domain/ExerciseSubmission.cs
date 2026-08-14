namespace Spracher.Modules.Exercises.Domain;

public sealed class ExerciseSubmission
{
    private ExerciseSubmission()
    {
    }

    private ExerciseSubmission(
        Guid attemptId,
        string responseJson,
        string gradingJson,
        bool isCorrect,
        int awardedPoints,
        int maxPoints,
        DateTimeOffset submittedAt)
    {
        Id = Guid.CreateVersion7();
        AttemptId = attemptId;
        ResponseJson = responseJson;
        GradingJson = gradingJson;
        IsCorrect = isCorrect;
        AwardedPoints = awardedPoints;
        MaxPoints = maxPoints;
        SubmittedAt = submittedAt;
    }

    public Guid Id { get; private set; }

    public Guid AttemptId { get; private set; }

    public string ResponseJson { get; private set; } = string.Empty;

    public string GradingJson { get; private set; } = string.Empty;

    public bool IsCorrect { get; private set; }

    public int AwardedPoints { get; private set; }

    public int MaxPoints { get; private set; }

    public DateTimeOffset SubmittedAt { get; private set; }

    public static ExerciseSubmission Create(
        Guid attemptId,
        string responseJson,
        string gradingJson,
        bool isCorrect,
        int awardedPoints,
        int maxPoints,
        DateTimeOffset submittedAt)
    {
        if (attemptId == Guid.Empty)
        {
            throw new ArgumentException("Attempt ID cannot be empty.", nameof(attemptId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(responseJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(gradingJson);

        if (maxPoints < 1 || awardedPoints < 0 || awardedPoints > maxPoints)
        {
            throw new ArgumentOutOfRangeException(
                nameof(awardedPoints),
                "Awarded points must be between zero and the maximum score.");
        }

        return new ExerciseSubmission(
            attemptId,
            responseJson,
            gradingJson,
            isCorrect,
            awardedPoints,
            maxPoints,
            submittedAt);
    }
}
