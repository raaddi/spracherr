namespace Spracher.Modules.Exercises.Domain;

public sealed class ExerciseSet
{
    private ExerciseSet()
    {
    }

    internal ExerciseSet(
        Guid id,
        string title,
        string? description,
        Guid? ownerUserId,
        ExerciseSetStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset? publishedAt)
    {
        Id = id;
        Title = title;
        Description = description;
        OwnerUserId = ownerUserId;
        Status = status;
        CreatedAt = createdAt;
        PublishedAt = publishedAt;
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public Guid? OwnerUserId { get; private set; }

    public ExerciseSetStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public static ExerciseSet CreateDraft(
        Guid ownerUserId,
        string title,
        string? description,
        DateTimeOffset createdAt)
    {
        if (ownerUserId == Guid.Empty)
        {
            throw new ArgumentException("Owner user ID cannot be empty.", nameof(ownerUserId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        return new ExerciseSet(
            Guid.CreateVersion7(),
            title.Trim(),
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            ownerUserId,
            ExerciseSetStatus.Draft,
            createdAt,
            publishedAt: null);
    }

    public void Publish(DateTimeOffset publishedAt)
    {
        if (Status != ExerciseSetStatus.Draft)
        {
            throw new InvalidOperationException("Only a draft exercise set can be published.");
        }

        Status = ExerciseSetStatus.Published;
        PublishedAt = publishedAt;
    }
}
