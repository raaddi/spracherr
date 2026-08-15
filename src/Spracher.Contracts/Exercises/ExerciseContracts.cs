using System.Text.Json;

namespace Spracher.Contracts.Exercises;

public sealed record ExerciseCatalogResponse(
    IReadOnlyList<ExerciseCatalogItemResponse> Items);

public sealed record ExerciseCatalogItemResponse(
    Guid DefinitionId,
    string TypeKey,
    string Title,
    string? Description,
    string Prompt,
    int VersionNumber);

public sealed record ExercisePlayResponse(
    Guid AttemptId,
    Guid DefinitionId,
    Guid ExerciseVersionId,
    string TypeKey,
    int SchemaVersion,
    string Title,
    string Prompt,
    JsonElement Payload,
    DateTimeOffset StartedAt);

public sealed record SubmitExerciseAttemptRequest(JsonElement Response);

public sealed record ExerciseResultResponse(
    Guid AttemptId,
    Guid ExerciseVersionId,
    bool IsCorrect,
    int AwardedPoints,
    int MaxPoints,
    string Feedback,
    DateTimeOffset CompletedAt);

public sealed record CreateExerciseDefinitionRequest(
    string TypeKey,
    string Title,
    string? Description,
    int SchemaVersion,
    string Prompt,
    JsonElement Definition);

public sealed record CreateExerciseVersionRequest(
    int SchemaVersion,
    string Prompt,
    JsonElement Definition);

public sealed record ExerciseAuthoringVersionResponse(
    Guid DefinitionId,
    Guid ExerciseVersionId,
    int VersionNumber,
    int SchemaVersion,
    string TypeKey,
    string Title,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PublishedAt);

public sealed record ExerciseSetCatalogResponse(
    IReadOnlyList<ExerciseSetCatalogItemResponse> Items);

public sealed record ExerciseSetCatalogItemResponse(
    Guid SetId,
    string Title,
    string? Description,
    IReadOnlyList<ExerciseSetItemResponse> Exercises);

public sealed record ExerciseSetItemResponse(
    Guid ItemId,
    int Position,
    Guid DefinitionId,
    Guid ExerciseVersionId,
    string TypeKey,
    string Title,
    string Prompt,
    int VersionNumber);

public sealed record CreateExerciseSetRequest(
    string Title,
    string? Description,
    IReadOnlyList<Guid> ExerciseVersionIds);

public sealed record ExerciseSetAuthoringResponse(
    Guid SetId,
    string Title,
    string Status,
    int ExerciseCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PublishedAt);
