using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Spracher.BuildingBlocks.Time;
using Spracher.Contracts.Exercises;
using Spracher.Modules.Exercises.Domain;
using Spracher.Modules.Exercises.Engine;
using Spracher.Persistence;

namespace Spracher.Modules.Exercises.Application;

internal sealed class ExerciseService(
    SpracherDbContext dbContext,
    ExerciseTypeRegistry typeRegistry,
    IClock clock)
{
    private const int MaxResponseBytes = 16 * 1024;

    public async Task<ExerciseCatalogResponse> GetCatalogAsync(
        CancellationToken cancellationToken)
    {
        var rows = await (
                from definition in dbContext.Set<ExerciseDefinition>().AsNoTracking()
                join version in dbContext.Set<ExerciseVersion>().AsNoTracking()
                    on definition.Id equals version.ExerciseDefinitionId
                where definition.ArchivedAt == null
                      && version.Status == ExerciseVersionStatus.Published
                orderby definition.Title, version.VersionNumber descending
                select new
                {
                    definition.Id,
                    definition.TypeKey,
                    definition.Title,
                    definition.Description,
                    version.Prompt,
                    version.VersionNumber,
                })
            .ToArrayAsync(cancellationToken);

        var items = rows
            .GroupBy(row => row.Id)
            .Select(group => group.First())
            .Select(row => new ExerciseCatalogItemResponse(
                row.Id,
                row.TypeKey,
                row.Title,
                row.Description,
                row.Prompt,
                row.VersionNumber))
            .ToArray();

        return new ExerciseCatalogResponse(items);
    }

    public async Task<ExerciseSetCatalogResponse> GetSetCatalogAsync(
        CancellationToken cancellationToken)
    {
        var rows = await (
                from set in dbContext.Set<ExerciseSet>().AsNoTracking()
                join item in dbContext.Set<ExerciseSetItem>().AsNoTracking()
                    on set.Id equals item.ExerciseSetId
                join version in dbContext.Set<ExerciseVersion>().AsNoTracking()
                    on item.ExerciseVersionId equals version.Id
                join definition in dbContext.Set<ExerciseDefinition>().AsNoTracking()
                    on version.ExerciseDefinitionId equals definition.Id
                where set.Status == ExerciseSetStatus.Published
                      && version.Status == ExerciseVersionStatus.Published
                      && definition.ArchivedAt == null
                orderby set.Title, item.Position
                select new
                {
                    SetId = set.Id,
                    set.Title,
                    set.Description,
                    ItemId = item.Id,
                    item.Position,
                    DefinitionId = definition.Id,
                    ExerciseVersionId = version.Id,
                    definition.TypeKey,
                    ExerciseTitle = definition.Title,
                    version.Prompt,
                    version.VersionNumber,
                })
            .ToArrayAsync(cancellationToken);

        var sets = rows
            .GroupBy(row => new { row.SetId, row.Title, row.Description })
            .Select(group => new ExerciseSetCatalogItemResponse(
                group.Key.SetId,
                group.Key.Title,
                group.Key.Description,
                group
                    .OrderBy(row => row.Position)
                    .Select(row => new ExerciseSetItemResponse(
                        row.ItemId,
                        row.Position,
                        row.DefinitionId,
                        row.ExerciseVersionId,
                        row.TypeKey,
                        row.ExerciseTitle,
                        row.Prompt,
                        row.VersionNumber))
                    .ToArray()))
            .ToArray();

        return new ExerciseSetCatalogResponse(sets);
    }

    public async Task<ExerciseResult<ExercisePlayResponse>> StartAttemptAsync(
        Guid userId,
        Guid definitionId,
        CancellationToken cancellationToken)
    {
        var row = await (
                from definition in dbContext.Set<ExerciseDefinition>().AsNoTracking()
                join version in dbContext.Set<ExerciseVersion>().AsNoTracking()
                    on definition.Id equals version.ExerciseDefinitionId
                where definition.Id == definitionId
                      && definition.ArchivedAt == null
                      && version.Status == ExerciseVersionStatus.Published
                orderby version.VersionNumber descending
                select new { Definition = definition, Version = version })
            .FirstOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            return ExerciseResult<ExercisePlayResponse>.NotFound();
        }

        return await StartAttemptForVersionAsync(
            userId,
            row.Definition,
            row.Version,
            exerciseSetItemId: null,
            cancellationToken);
    }

    public async Task<ExerciseResult<ExercisePlayResponse>> StartSetItemAttemptAsync(
        Guid userId,
        Guid setId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var row = await (
                from set in dbContext.Set<ExerciseSet>().AsNoTracking()
                join item in dbContext.Set<ExerciseSetItem>().AsNoTracking()
                    on set.Id equals item.ExerciseSetId
                join version in dbContext.Set<ExerciseVersion>().AsNoTracking()
                    on item.ExerciseVersionId equals version.Id
                join definition in dbContext.Set<ExerciseDefinition>().AsNoTracking()
                    on version.ExerciseDefinitionId equals definition.Id
                where set.Id == setId
                      && item.Id == itemId
                      && set.Status == ExerciseSetStatus.Published
                      && version.Status == ExerciseVersionStatus.Published
                      && definition.ArchivedAt == null
                select new { Item = item, Definition = definition, Version = version })
            .SingleOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            return ExerciseResult<ExercisePlayResponse>.NotFound();
        }

        return await StartAttemptForVersionAsync(
            userId,
            row.Definition,
            row.Version,
            row.Item.Id,
            cancellationToken);
    }

    private async Task<ExerciseResult<ExercisePlayResponse>> StartAttemptForVersionAsync(
        Guid userId,
        ExerciseDefinition definition,
        ExerciseVersion version,
        Guid? exerciseSetItemId,
        CancellationToken cancellationToken)
    {
        var handler = typeRegistry.GetRequired(definition.TypeKey, version.SchemaVersion);
        var validation = handler.ValidateDefinition(version.DefinitionJson);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(
                $"Published exercise version '{version.Id}' has an invalid definition.");
        }

        var attempt = exerciseSetItemId.HasValue
            ? ExerciseAttempt.StartFromSet(
                userId,
                version.Id,
                exerciseSetItemId.Value,
                clock.UtcNow)
            : ExerciseAttempt.Start(userId, version.Id, clock.UtcNow);
        dbContext.Add(attempt);
        await dbContext.SaveChangesAsync(cancellationToken);

        using var payloadDocument = JsonDocument.Parse(
            handler.CreateClientPayload(version.DefinitionJson));
        return ExerciseResult<ExercisePlayResponse>.Success(
            new ExercisePlayResponse(
                attempt.Id,
                definition.Id,
                version.Id,
                definition.TypeKey,
                version.SchemaVersion,
                definition.Title,
                version.Prompt,
                payloadDocument.RootElement.Clone(),
                attempt.StartedAt));
    }

    public async Task<ExerciseResult<ExerciseResultResponse>> SubmitAttemptAsync(
        Guid userId,
        Guid attemptId,
        SubmitExerciseAttemptRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Response.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return ExerciseResult<ExerciseResultResponse>.Validation(
                "response",
                "Submit an answer.");
        }

        var responseJson = request.Response.GetRawText();
        if (Encoding.UTF8.GetByteCount(responseJson) > MaxResponseBytes)
        {
            return ExerciseResult<ExerciseResultResponse>.Validation(
                "response",
                "The submitted answer is too large.");
        }

        var row = await (
                from attempt in dbContext.Set<ExerciseAttempt>()
                join version in dbContext.Set<ExerciseVersion>().AsNoTracking()
                    on attempt.ExerciseVersionId equals version.Id
                join definition in dbContext.Set<ExerciseDefinition>().AsNoTracking()
                    on version.ExerciseDefinitionId equals definition.Id
                where attempt.Id == attemptId && attempt.UserId == userId
                select new { Attempt = attempt, Version = version, Definition = definition })
            .SingleOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            return ExerciseResult<ExerciseResultResponse>.NotFound();
        }

        if (row.Attempt.Status != ExerciseAttemptStatus.InProgress)
        {
            return ExerciseResult<ExerciseResultResponse>.Conflict(
                "attemptId",
                "This exercise attempt has already been submitted.");
        }

        var handler = typeRegistry.GetRequired(
            row.Definition.TypeKey,
            row.Version.SchemaVersion);
        var grading = handler.Grade(row.Version.DefinitionJson, responseJson);
        if (!grading.IsAccepted)
        {
            return ExerciseResult<ExerciseResultResponse>.Validation(grading.Errors);
        }

        var completedAt = clock.UtcNow;
        row.Attempt.Complete(grading.AwardedPoints, grading.MaxPoints, completedAt);
        var gradingJson = JsonSerializer.Serialize(new
        {
            handler = row.Definition.TypeKey,
            schemaVersion = row.Version.SchemaVersion,
            grading.IsCorrect,
            grading.AwardedPoints,
            grading.MaxPoints,
            grading.Feedback,
        });
        dbContext.Add(ExerciseSubmission.Create(
            row.Attempt.Id,
            responseJson,
            gradingJson,
            grading.IsCorrect,
            grading.AwardedPoints,
            grading.MaxPoints,
            completedAt));
        await dbContext.SaveChangesAsync(cancellationToken);

        return ExerciseResult<ExerciseResultResponse>.Success(
            new ExerciseResultResponse(
                row.Attempt.Id,
                row.Version.Id,
                grading.IsCorrect,
                grading.AwardedPoints,
                grading.MaxPoints,
                grading.Feedback,
                completedAt));
    }
}
