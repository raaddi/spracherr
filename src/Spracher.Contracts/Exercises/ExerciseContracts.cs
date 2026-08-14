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
