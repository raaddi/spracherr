namespace Spracher.Modules.Exercises.Domain;

public sealed class ExerciseVersion
{
    private ExerciseVersion()
    {
    }

    internal ExerciseVersion(
        Guid id,
        Guid exerciseDefinitionId,
        int versionNumber,
        int schemaVersion,
        string prompt,
        string definitionJson,
        ExerciseVersionStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset? publishedAt)
    {
        Id = id;
        ExerciseDefinitionId = exerciseDefinitionId;
        VersionNumber = versionNumber;
        SchemaVersion = schemaVersion;
        Prompt = prompt;
        DefinitionJson = definitionJson;
        Status = status;
        CreatedAt = createdAt;
        PublishedAt = publishedAt;
    }

    public Guid Id { get; private set; }

    public Guid ExerciseDefinitionId { get; private set; }

    public int VersionNumber { get; private set; }

    public int SchemaVersion { get; private set; }

    public string Prompt { get; private set; } = string.Empty;

    public string DefinitionJson { get; private set; } = string.Empty;

    public ExerciseVersionStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public static ExerciseVersion CreateDraft(
        Guid exerciseDefinitionId,
        int versionNumber,
        int schemaVersion,
        string prompt,
        string definitionJson,
        DateTimeOffset createdAt)
    {
        if (exerciseDefinitionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Exercise definition ID cannot be empty.",
                nameof(exerciseDefinitionId));
        }

        if (versionNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(versionNumber),
                "Version number must be positive.");
        }

        if (schemaVersion < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(schemaVersion),
                "Schema version must be positive.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionJson);

        return new ExerciseVersion(
            Guid.CreateVersion7(),
            exerciseDefinitionId,
            versionNumber,
            schemaVersion,
            prompt.Trim(),
            definitionJson,
            ExerciseVersionStatus.Draft,
            createdAt,
            publishedAt: null);
    }

    public void Publish(DateTimeOffset publishedAt)
    {
        if (Status != ExerciseVersionStatus.Draft)
        {
            throw new InvalidOperationException("Only a draft exercise version can be published.");
        }

        Status = ExerciseVersionStatus.Published;
        PublishedAt = publishedAt;
    }
}
