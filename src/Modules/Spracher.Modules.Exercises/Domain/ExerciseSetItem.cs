namespace Spracher.Modules.Exercises.Domain;

public sealed class ExerciseSetItem
{
    private ExerciseSetItem()
    {
    }

    internal ExerciseSetItem(
        Guid id,
        Guid exerciseSetId,
        Guid exerciseVersionId,
        int position)
    {
        Id = id;
        ExerciseSetId = exerciseSetId;
        ExerciseVersionId = exerciseVersionId;
        Position = position;
    }

    public Guid Id { get; private set; }

    public Guid ExerciseSetId { get; private set; }

    public Guid ExerciseVersionId { get; private set; }

    public int Position { get; private set; }

    public static ExerciseSetItem Create(
        Guid exerciseSetId,
        Guid exerciseVersionId,
        int position)
    {
        if (exerciseSetId == Guid.Empty)
        {
            throw new ArgumentException("Exercise set ID cannot be empty.", nameof(exerciseSetId));
        }

        if (exerciseVersionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Exercise version ID cannot be empty.",
                nameof(exerciseVersionId));
        }

        if (position < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(position),
                "Exercise set position must be positive.");
        }

        return new ExerciseSetItem(
            Guid.CreateVersion7(),
            exerciseSetId,
            exerciseVersionId,
            position);
    }
}
