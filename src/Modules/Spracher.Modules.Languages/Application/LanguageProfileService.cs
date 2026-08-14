using Microsoft.EntityFrameworkCore;
using Spracher.BuildingBlocks.Time;
using Spracher.Contracts.Languages;
using Spracher.Modules.Languages.Domain;
using Spracher.Persistence;

namespace Spracher.Modules.Languages.Application;

internal sealed class LanguageProfileService(
    SpracherDbContext dbContext,
    IClock clock)
{
    public async Task<IReadOnlyList<LanguageResponse>> GetCatalogAsync(
        CancellationToken cancellationToken) =>
        await dbContext.Set<Language>()
            .AsNoTracking()
            .Where(language => language.IsActive)
            .OrderBy(language => language.Name)
            .Select(language => new LanguageResponse(
                language.Id,
                language.Code,
                language.Name,
                language.NativeName,
                language.TextDirection == TextDirection.LeftToRight ? "ltr" : "rtl"))
            .ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyList<UserLanguageProfileResponse>> GetUserLanguagesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var languages = await dbContext.Set<Language>()
            .AsNoTracking()
            .Where(language => language.IsActive)
            .OrderBy(language => language.Name)
            .ToArrayAsync(cancellationToken);
        var profiles = await dbContext.Set<UserLanguageProfile>()
            .AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .ToDictionaryAsync(profile => profile.LanguageId, cancellationToken);

        return languages
            .Select(language =>
            {
                profiles.TryGetValue(language.Id, out var profile);
                return new UserLanguageProfileResponse(
                    language.Id,
                    language.Code,
                    language.Name,
                    language.NativeName,
                    profile?.IsNative ?? false,
                    profile?.IsLearning ?? false,
                    profile?.CurrentCefrLevel?.ToString(),
                    profile?.StartedAt);
            })
            .ToArray();
    }

    public async Task<LanguageUpdateResult> UpdateUserLanguagesAsync(
        Guid userId,
        UpdateUserLanguagesRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return LanguageUpdateResult.Invalid(validationErrors);
        }

        var requestedSelections = request.Languages.ToDictionary(
            selection => selection.LanguageId);
        var activeLanguageIds = await dbContext.Set<Language>()
            .AsNoTracking()
            .Where(language =>
                language.IsActive && requestedSelections.Keys.Contains(language.Id))
            .Select(language => language.Id)
            .ToArrayAsync(cancellationToken);

        var unknownIds = requestedSelections.Keys.Except(activeLanguageIds).ToArray();
        if (unknownIds.Length > 0)
        {
            return LanguageUpdateResult.Invalid(
                new Dictionary<string, string[]>
                {
                    ["languages"] = ["One or more selected languages are unavailable."],
                });
        }

        var existingProfiles = await dbContext.Set<UserLanguageProfile>()
            .Where(profile => profile.UserId == userId)
            .ToArrayAsync(cancellationToken);
        var now = clock.UtcNow;

        foreach (var existingProfile in existingProfiles)
        {
            if (requestedSelections.TryGetValue(
                    existingProfile.LanguageId,
                    out var selection))
            {
                existingProfile.UpdateSelection(
                    selection.IsNative,
                    selection.IsLearning,
                    now);
                requestedSelections.Remove(existingProfile.LanguageId);
            }
            else
            {
                dbContext.Remove(existingProfile);
            }
        }

        foreach (var selection in requestedSelections.Values)
        {
            dbContext.Add(UserLanguageProfile.Create(
                userId,
                selection.LanguageId,
                selection.IsNative,
                selection.IsLearning,
                now));
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return LanguageUpdateResult.Success(
            await GetUserLanguagesAsync(userId, cancellationToken));
    }

    private static Dictionary<string, string[]> Validate(UpdateUserLanguagesRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request.Languages is null || request.Languages.Count == 0)
        {
            errors["languages"] = ["Select at least one learning language."];
            return errors;
        }

        if (request.Languages.Count > 20)
        {
            errors["languages"] = ["No more than 20 languages can be selected."];
        }

        if (request.Languages.GroupBy(selection => selection.LanguageId).Any(group => group.Count() > 1))
        {
            errors["languages"] = ["Each language can be selected only once."];
        }
        else if (request.Languages.Any(selection => !selection.IsNative && !selection.IsLearning))
        {
            errors["languages"] = ["Each selection must be native, learning, or both."];
        }
        else if (!request.Languages.Any(selection => selection.IsLearning))
        {
            errors["languages"] = ["Select at least one learning language."];
        }

        return errors;
    }
}

internal sealed record LanguageUpdateResult(
    bool Succeeded,
    IReadOnlyList<UserLanguageProfileResponse> Languages,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public static LanguageUpdateResult Success(
        IReadOnlyList<UserLanguageProfileResponse> languages) =>
        new(true, languages, new Dictionary<string, string[]>());

    public static LanguageUpdateResult Invalid(
        IReadOnlyDictionary<string, string[]> errors) =>
        new(false, [], errors);
}
