using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Spracher.BuildingBlocks.Time;
using Spracher.Contracts.Exercises;
using Spracher.Modules.Exercises.Domain;
using Spracher.Modules.Exercises.Engine;
using Spracher.Persistence;

namespace Spracher.Modules.Exercises.Application;

internal sealed class ExerciseAuthoringService(
    SpracherDbContext dbContext,
    ExerciseTypeRegistry typeRegistry,
    IClock clock)
{
    private const int MaxDefinitionBytes = 64 * 1024;

    public async Task<ExerciseResult<ExerciseAuthoringVersionResponse>> CreateDefinitionAsync(
        Guid ownerUserId,
        CreateExerciseDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var metadataError = ValidateMetadata(
            request.TypeKey,
            request.Title,
            request.Description,
            request.SchemaVersion,
            request.Prompt);
        if (metadataError is not null)
        {
            return ExerciseResult<ExerciseAuthoringVersionResponse>.Validation(
                metadataError.Value.Key,
                metadataError.Value.Message);
        }

        var definitionValidation = ValidatePayload(
            request.TypeKey.Trim(),
            request.SchemaVersion,
            request.Definition);
        if (definitionValidation is not null)
        {
            return ExerciseResult<ExerciseAuthoringVersionResponse>.Validation(
                definitionValidation);
        }

        var now = clock.UtcNow;
        var definition = ExerciseDefinition.CreateOwned(
            ownerUserId,
            request.TypeKey,
            request.Title,
            request.Description,
            now);
        var version = ExerciseVersion.CreateDraft(
            definition.Id,
            versionNumber: 1,
            request.SchemaVersion,
            request.Prompt,
            request.Definition.GetRawText(),
            now);
        dbContext.Add(definition);
        dbContext.Add(version);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ExerciseResult<ExerciseAuthoringVersionResponse>.Success(
            MapResponse(definition, version));
    }

    public async Task<ExerciseResult<ExerciseAuthoringVersionResponse>> CreateVersionAsync(
        Guid definitionId,
        CreateExerciseVersionRequest request,
        CancellationToken cancellationToken)
    {
        var definition = await dbContext.Set<ExerciseDefinition>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == definitionId && item.ArchivedAt == null,
                cancellationToken);
        if (definition is null)
        {
            return ExerciseResult<ExerciseAuthoringVersionResponse>.NotFound();
        }

        var metadataError = ValidateVersionMetadata(request.SchemaVersion, request.Prompt);
        if (metadataError is not null)
        {
            return ExerciseResult<ExerciseAuthoringVersionResponse>.Validation(
                metadataError.Value.Key,
                metadataError.Value.Message);
        }

        var definitionValidation = ValidatePayload(
            definition.TypeKey,
            request.SchemaVersion,
            request.Definition);
        if (definitionValidation is not null)
        {
            return ExerciseResult<ExerciseAuthoringVersionResponse>.Validation(
                definitionValidation);
        }

        var hasDraft = await dbContext.Set<ExerciseVersion>()
            .AnyAsync(
                version => version.ExerciseDefinitionId == definitionId
                           && version.Status == ExerciseVersionStatus.Draft,
                cancellationToken);
        if (hasDraft)
        {
            return ExerciseResult<ExerciseAuthoringVersionResponse>.Conflict(
                "definitionId",
                "Publish or discard the existing draft before creating a new version.");
        }

        var latestVersion = await dbContext.Set<ExerciseVersion>()
            .Where(version => version.ExerciseDefinitionId == definitionId)
            .MaxAsync(version => version.VersionNumber, cancellationToken);
        var draft = ExerciseVersion.CreateDraft(
            definitionId,
            latestVersion + 1,
            request.SchemaVersion,
            request.Prompt,
            request.Definition.GetRawText(),
            clock.UtcNow);
        dbContext.Add(draft);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ExerciseResult<ExerciseAuthoringVersionResponse>.Success(
            MapResponse(definition, draft));
    }

    public async Task<ExerciseResult<ExerciseAuthoringVersionResponse>> PublishVersionAsync(
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var version = await dbContext.Set<ExerciseVersion>()
            .SingleOrDefaultAsync(item => item.Id == versionId, cancellationToken);
        if (version is null)
        {
            return ExerciseResult<ExerciseAuthoringVersionResponse>.NotFound();
        }

        var definition = await dbContext.Set<ExerciseDefinition>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == version.ExerciseDefinitionId
                        && item.ArchivedAt == null,
                cancellationToken);
        if (definition is null)
        {
            return ExerciseResult<ExerciseAuthoringVersionResponse>.NotFound();
        }

        if (version.Status != ExerciseVersionStatus.Draft)
        {
            return ExerciseResult<ExerciseAuthoringVersionResponse>.Conflict(
                "versionId",
                "Only a draft exercise version can be published.");
        }

        if (!typeRegistry.TryGet(
                definition.TypeKey,
                version.SchemaVersion,
                out var handler)
            || handler is null)
        {
            return ExerciseResult<ExerciseAuthoringVersionResponse>.Validation(
                "schemaVersion",
                "No compatible exercise handler is registered.");
        }

        var validation = handler.ValidateDefinition(version.DefinitionJson);
        if (!validation.IsValid)
        {
            return ExerciseResult<ExerciseAuthoringVersionResponse>.Validation(validation.Errors);
        }

        version.Publish(clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ExerciseResult<ExerciseAuthoringVersionResponse>.Success(
            MapResponse(definition, version));
    }

    public async Task<ExerciseResult<ExerciseSetAuthoringResponse>> CreateSetAsync(
        Guid ownerUserId,
        CreateExerciseSetRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > 200)
        {
            return ExerciseResult<ExerciseSetAuthoringResponse>.Validation(
                "title",
                "Title is required and cannot exceed 200 characters.");
        }

        if (request.Description?.Trim().Length > 1000)
        {
            return ExerciseResult<ExerciseSetAuthoringResponse>.Validation(
                "description",
                "Description cannot exceed 1000 characters.");
        }

        if (request.ExerciseVersionIds is null
            || request.ExerciseVersionIds.Count is < 1 or > 50
            || request.ExerciseVersionIds.Any(id => id == Guid.Empty)
            || request.ExerciseVersionIds.Distinct().Count()
                != request.ExerciseVersionIds.Count)
        {
            return ExerciseResult<ExerciseSetAuthoringResponse>.Validation(
                "exerciseVersionIds",
                "Provide between 1 and 50 unique exercise version IDs.");
        }

        var validVersionCount = await (
                from version in dbContext.Set<ExerciseVersion>().AsNoTracking()
                join definition in dbContext.Set<ExerciseDefinition>().AsNoTracking()
                    on version.ExerciseDefinitionId equals definition.Id
                where request.ExerciseVersionIds.Contains(version.Id)
                      && version.Status == ExerciseVersionStatus.Published
                      && definition.ArchivedAt == null
                select version.Id)
            .CountAsync(cancellationToken);
        if (validVersionCount != request.ExerciseVersionIds.Count)
        {
            return ExerciseResult<ExerciseSetAuthoringResponse>.Validation(
                "exerciseVersionIds",
                "Every set item must reference an active, published exercise version.");
        }

        var set = ExerciseSet.CreateDraft(
            ownerUserId,
            request.Title,
            request.Description,
            clock.UtcNow);
        var items = request.ExerciseVersionIds
            .Select((versionId, index) => ExerciseSetItem.Create(set.Id, versionId, index + 1))
            .ToArray();
        dbContext.Add(set);
        dbContext.AddRange(items);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ExerciseResult<ExerciseSetAuthoringResponse>.Success(
            MapSetResponse(set, items.Length));
    }

    public async Task<ExerciseResult<ExerciseSetAuthoringResponse>> PublishSetAsync(
        Guid setId,
        CancellationToken cancellationToken)
    {
        var set = await dbContext.Set<ExerciseSet>()
            .SingleOrDefaultAsync(item => item.Id == setId, cancellationToken);
        if (set is null)
        {
            return ExerciseResult<ExerciseSetAuthoringResponse>.NotFound();
        }

        if (set.Status != ExerciseSetStatus.Draft)
        {
            return ExerciseResult<ExerciseSetAuthoringResponse>.Conflict(
                "setId",
                "Only a draft exercise set can be published.");
        }

        var itemCount = await dbContext.Set<ExerciseSetItem>()
            .CountAsync(item => item.ExerciseSetId == setId, cancellationToken);
        var validItemCount = await (
                from item in dbContext.Set<ExerciseSetItem>().AsNoTracking()
                join version in dbContext.Set<ExerciseVersion>().AsNoTracking()
                    on item.ExerciseVersionId equals version.Id
                join definition in dbContext.Set<ExerciseDefinition>().AsNoTracking()
                    on version.ExerciseDefinitionId equals definition.Id
                where item.ExerciseSetId == setId
                      && version.Status == ExerciseVersionStatus.Published
                      && definition.ArchivedAt == null
                select item.Id)
            .CountAsync(cancellationToken);
        if (itemCount == 0 || itemCount != validItemCount)
        {
            return ExerciseResult<ExerciseSetAuthoringResponse>.Validation(
                "items",
                "Every set item must reference an active, published exercise version.");
        }

        set.Publish(clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ExerciseResult<ExerciseSetAuthoringResponse>.Success(
            MapSetResponse(set, itemCount));
    }

    private IReadOnlyDictionary<string, string[]>? ValidatePayload(
        string typeKey,
        int schemaVersion,
        JsonElement definition)
    {
        if (definition.ValueKind != JsonValueKind.Object)
        {
            return Error("definition", "The exercise definition must be a JSON object.");
        }

        var definitionJson = definition.GetRawText();
        if (Encoding.UTF8.GetByteCount(definitionJson) > MaxDefinitionBytes)
        {
            return Error("definition", "The exercise definition is too large.");
        }

        if (!typeRegistry.TryGet(typeKey, schemaVersion, out var handler)
            || handler is null)
        {
            return Error(
                "schemaVersion",
                "No compatible exercise handler is registered.");
        }

        var validation = handler.ValidateDefinition(definitionJson);
        return validation.IsValid ? null : validation.Errors;
    }

    private static (string Key, string Message)? ValidateMetadata(
        string typeKey,
        string title,
        string? description,
        int schemaVersion,
        string prompt)
    {
        if (string.IsNullOrWhiteSpace(typeKey) || typeKey.Trim().Length > 80)
        {
            return ("typeKey", "Type key is required and cannot exceed 80 characters.");
        }

        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 200)
        {
            return ("title", "Title is required and cannot exceed 200 characters.");
        }

        if (description?.Trim().Length > 1000)
        {
            return ("description", "Description cannot exceed 1000 characters.");
        }

        return ValidateVersionMetadata(schemaVersion, prompt);
    }

    private static (string Key, string Message)? ValidateVersionMetadata(
        int schemaVersion,
        string prompt)
    {
        if (schemaVersion < 1)
        {
            return ("schemaVersion", "Schema version must be positive.");
        }

        return string.IsNullOrWhiteSpace(prompt) || prompt.Trim().Length > 2000
            ? ("prompt", "Prompt is required and cannot exceed 2000 characters.")
            : null;
    }

    private static Dictionary<string, string[]> Error(string key, string message) =>
        new(StringComparer.Ordinal) { [key] = [message] };

    private static ExerciseAuthoringVersionResponse MapResponse(
        ExerciseDefinition definition,
        ExerciseVersion version) =>
        new(
            definition.Id,
            version.Id,
            version.VersionNumber,
            version.SchemaVersion,
            definition.TypeKey,
            definition.Title,
            version.Status.ToString(),
            version.CreatedAt,
            version.PublishedAt);

    private static ExerciseSetAuthoringResponse MapSetResponse(
        ExerciseSet set,
        int exerciseCount) =>
        new(
            set.Id,
            set.Title,
            set.Status.ToString(),
            exerciseCount,
            set.CreatedAt,
            set.PublishedAt);
}
