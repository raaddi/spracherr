namespace Spracher.Modules.Exercises.Domain;

public sealed class ExerciseDefinition
{
    private ExerciseDefinition()
    {
    }

    internal ExerciseDefinition(
        Guid id,
        string typeKey,
        string title,
        string? description,
        DateTimeOffset createdAt)
    {
        Id = id;
        TypeKey = typeKey;
        Title = title;
        Description = description;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public string TypeKey { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ArchivedAt { get; private set; }

    public static ExerciseDefinition Create(
        string typeKey,
        string title,
        string? description,
        DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return new ExerciseDefinition(
            Guid.CreateVersion7(),
            typeKey.Trim(),
            title.Trim(),
            NormalizeOptional(description),
            createdAt);
    }

    public void Archive(DateTimeOffset archivedAt)
    {
        ArchivedAt ??= archivedAt;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
