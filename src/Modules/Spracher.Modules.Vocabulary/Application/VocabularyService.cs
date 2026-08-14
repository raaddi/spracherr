using Microsoft.EntityFrameworkCore;
using Npgsql;
using Spracher.BuildingBlocks.Languages;
using Spracher.BuildingBlocks.Time;
using Spracher.Contracts.Vocabulary;
using Spracher.Modules.Vocabulary.Domain;
using Spracher.Persistence;

namespace Spracher.Modules.Vocabulary.Application;

internal sealed class VocabularyService(
    SpracherDbContext dbContext,
    ILanguageCatalogReader languageCatalog,
    IClock clock)
{
    public async Task<VocabularyResult<VocabularySearchResponse>> SearchAsync(
        Guid? userId,
        Guid languageId,
        string? query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (languageId == Guid.Empty)
        {
            return VocabularyResult<VocabularySearchResponse>.Validation(
                "languageId",
                "Select an active language.");
        }

        var pagingError = ValidatePaging(page, pageSize);
        if (pagingError is not null)
        {
            return VocabularyResult<VocabularySearchResponse>.Validation("page", pagingError);
        }

        if (query?.Length > 100)
        {
            return VocabularyResult<VocabularySearchResponse>.Validation(
                "query",
                "Search query cannot exceed 100 characters.");
        }

        var languages = await languageCatalog.GetActiveByIdsAsync(
            [languageId],
            cancellationToken);
        if (!languages.TryGetValue(languageId, out var language))
        {
            return VocabularyResult<VocabularySearchResponse>.Validation(
                "languageId",
                "Select an active language.");
        }

        var normalizedQuery = string.IsNullOrWhiteSpace(query)
            ? null
            : VocabularyTextNormalizer.NormalizeLemma(query);
        var lexemes = VisibleLexemes(userId)
            .Where(lexeme => lexeme.LanguageId == languageId);
        if (normalizedQuery is not null)
        {
            lexemes = lexemes.Where(lexeme =>
                lexeme.NormalizedLemma.StartsWith(normalizedQuery));
        }

        var visibleSenses = VisibleSenses(userId);
        var totalCount = await lexemes.CountAsync(cancellationToken);
        var rows = await lexemes
            .OrderBy(lexeme => lexeme.NormalizedLemma)
            .ThenBy(lexeme => lexeme.PartOfSpeech)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(lexeme => new
            {
                lexeme.Id,
                lexeme.Lemma,
                lexeme.LanguageId,
                lexeme.PartOfSpeech,
                lexeme.CefrLevel,
                lexeme.FrequencyRank,
                lexeme.Visibility,
                SenseCount = visibleSenses
                    .Count(sense => sense.LexemeId == lexeme.Id),
            })
            .ToArrayAsync(cancellationToken);

        var items = rows.Select(row => new VocabularySearchItemResponse(
            row.Id,
            row.Lemma,
            row.LanguageId,
            language.Code,
            language.Name,
            row.PartOfSpeech.ToString(),
            row.CefrLevel?.ToString(),
            row.FrequencyRank,
            row.Visibility == VocabularyVisibility.Private,
            row.SenseCount)).ToArray();

        return VocabularyResult<VocabularySearchResponse>.Success(
            new VocabularySearchResponse(page, pageSize, totalCount, items));
    }

    public async Task<VocabularyResult<VocabularyDetailsResponse>> GetDetailsAsync(
        Guid? userId,
        Guid lexemeId,
        CancellationToken cancellationToken)
    {
        var lexeme = await VisibleLexemes(userId)
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == lexemeId, cancellationToken);
        if (lexeme is null)
        {
            return VocabularyResult<VocabularyDetailsResponse>.NotFound();
        }

        var senses = await VisibleSenses(userId)
            .AsNoTracking()
            .Where(sense => sense.LexemeId == lexemeId)
            .OrderBy(sense => sense.Definition)
            .ToArrayAsync(cancellationToken);
        var conceptIds = senses.Select(sense => sense.ConceptId).Distinct().ToArray();
        var equivalentSenses = conceptIds.Length == 0
            ? []
            : await VisibleSenses(userId)
                .AsNoTracking()
                .Where(sense =>
                    conceptIds.Contains(sense.ConceptId) && sense.LexemeId != lexemeId)
                .ToArrayAsync(cancellationToken);
        var equivalentLexemeIds = equivalentSenses
            .Select(sense => sense.LexemeId)
            .Distinct()
            .ToArray();
        var equivalentLexemes = equivalentLexemeIds.Length == 0
            ? []
            : await VisibleLexemes(userId)
                .AsNoTracking()
                .Where(item => equivalentLexemeIds.Contains(item.Id))
                .ToArrayAsync(cancellationToken);

        var forms = await dbContext.Set<WordForm>()
            .AsNoTracking()
            .Where(form => form.LexemeId == lexemeId)
            .OrderBy(form => form.Form)
            .ToArrayAsync(cancellationToken);
        var pronunciations = await dbContext.Set<Pronunciation>()
            .AsNoTracking()
            .Where(pronunciation => pronunciation.LexemeId == lexemeId)
            .OrderBy(pronunciation => pronunciation.Scheme)
            .ToArrayAsync(cancellationToken);
        var features = await dbContext.Set<LexemeFeature>()
            .AsNoTracking()
            .Where(feature => feature.LexemeId == lexemeId)
            .OrderBy(feature => feature.Key)
            .ToArrayAsync(cancellationToken);

        var senseIds = senses.Select(sense => sense.Id).ToArray();
        var usages = senseIds.Length == 0
            ? []
            : await dbContext.Set<ExampleUsage>()
                .AsNoTracking()
                .Where(usage => senseIds.Contains(usage.LexemeSenseId))
                .ToArrayAsync(cancellationToken);
        var exampleIds = usages.Select(usage => usage.ExampleSentenceId).Distinct().ToArray();
        var examples = exampleIds.Length == 0
            ? []
            : await VisibleExamples(userId)
                .AsNoTracking()
                .Where(example => exampleIds.Contains(example.Id))
                .ToArrayAsync(cancellationToken);

        var languageIds = equivalentLexemes
            .Select(item => item.LanguageId)
            .Append(lexeme.LanguageId)
            .Concat(senses.Select(sense => sense.DefinitionLanguageId))
            .Concat(examples.Select(example => example.LanguageId))
            .Distinct()
            .ToArray();
        var languages = await languageCatalog.GetActiveByIdsAsync(
            languageIds,
            cancellationToken);
        if (!languages.TryGetValue(lexeme.LanguageId, out var language))
        {
            return VocabularyResult<VocabularyDetailsResponse>.NotFound();
        }

        var equivalentLexemesById = equivalentLexemes.ToDictionary(item => item.Id);
        var examplesById = examples.ToDictionary(item => item.Id);
        var responseSenses = senses.Select(sense => new LexemeSenseResponse(
            sense.Id,
            sense.ConceptId,
            sense.Definition,
            GetLanguageCode(languages, sense.DefinitionLanguageId),
            sense.Register,
            (sense.CefrLevelOverride ?? lexeme.CefrLevel)?.ToString(),
            equivalentSenses
                .Where(candidate => candidate.ConceptId == sense.ConceptId)
                .Select(candidate => equivalentLexemesById.GetValueOrDefault(candidate.LexemeId) is { } equivalent
                    ? new EquivalentLexemeResponse(
                        equivalent.Id,
                        candidate.Id,
                        equivalent.Lemma,
                        GetLanguageCode(languages, equivalent.LanguageId),
                        equivalent.PartOfSpeech.ToString())
                    : null)
                .OfType<EquivalentLexemeResponse>()
                .OrderBy(equivalent => equivalent.LanguageCode)
                .ThenBy(equivalent => equivalent.Lemma)
                .ToArray(),
            usages
                .Where(usage => usage.LexemeSenseId == sense.Id)
                .Select(usage => examplesById.GetValueOrDefault(usage.ExampleSentenceId) is { } example
                    ? new ExampleSentenceResponse(
                        example.Id,
                        example.Text,
                        GetLanguageCode(languages, example.LanguageId),
                        usage.HighlightStart,
                        usage.HighlightLength)
                    : null)
                .OfType<ExampleSentenceResponse>()
                .ToArray())).ToArray();

        return VocabularyResult<VocabularyDetailsResponse>.Success(
            new VocabularyDetailsResponse(
                lexeme.Id,
                lexeme.Lemma,
                lexeme.LanguageId,
                language.Code,
                language.Name,
                lexeme.PartOfSpeech.ToString(),
                lexeme.CefrLevel?.ToString(),
                lexeme.FrequencyRank,
                lexeme.Notes,
                lexeme.Visibility == VocabularyVisibility.Private,
                lexeme.SourceType.ToString(),
                lexeme.SourceReference,
                forms.Select(form => new WordFormResponse(form.Form, form.GrammarTags)).ToArray(),
                pronunciations.Select(pronunciation => new PronunciationResponse(
                    pronunciation.Scheme,
                    pronunciation.Value,
                    pronunciation.Region,
                    pronunciation.AudioAssetReference)).ToArray(),
                features.Select(feature => new LexemeFeatureResponse(
                    feature.Key,
                    feature.Value)).ToArray(),
                responseSenses));
    }

    public async Task<VocabularyResult<UserVocabularyItemResponse>> AddItemAsync(
        Guid userId,
        AddVocabularyItemRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var sense = await VisibleSenses(userId)
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == request.LexemeSenseId,
                cancellationToken);
        if (sense is null)
        {
            return VocabularyResult<UserVocabularyItemResponse>.NotFound();
        }

        var existing = await dbContext.Set<UserVocabularyItem>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.UserId == userId && item.LexemeSenseId == sense.Id,
                cancellationToken);
        if (existing is not null)
        {
            return await GetItemResponseAsync(existing.Id, userId, cancellationToken);
        }

        var item = UserVocabularyItem.Create(userId, sense.Id, clock.UtcNow);
        dbContext.Add(item);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(
            exception,
            "IX_UserVocabularyItems_UserId_LexemeSenseId"))
        {
            dbContext.ChangeTracker.Clear();
            var existingId = await dbContext.Set<UserVocabularyItem>()
                .Where(candidate =>
                    candidate.UserId == userId && candidate.LexemeSenseId == sense.Id)
                .Select(candidate => candidate.Id)
                .SingleAsync(cancellationToken);
            return await GetItemResponseAsync(existingId, userId, cancellationToken);
        }

        return await GetItemResponseAsync(item.Id, userId, cancellationToken);
    }

    public async Task<VocabularyResult<UserVocabularyItemResponse>> CreatePrivateAsync(
        Guid userId,
        CreatePrivateVocabularyRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validation = ValidatePrivateRequest(request);
        if (validation is not null)
        {
            return VocabularyResult<UserVocabularyItemResponse>.Validation(
                validation.Value.Key,
                validation.Value.Message);
        }

        var languageIds = new[] { request.LanguageId, request.DefinitionLanguageId }
            .Distinct()
            .ToArray();
        var languages = await languageCatalog.GetActiveByIdsAsync(
            languageIds,
            cancellationToken);
        if (languages.Count != languageIds.Length)
        {
            return VocabularyResult<UserVocabularyItemResponse>.Validation(
                "languageId",
                "Select active languages for the word and definition.");
        }

        if (!TryParseNamedEnum(request.PartOfSpeech, out PartOfSpeech partOfSpeech))
        {
            return VocabularyResult<UserVocabularyItemResponse>.Validation(
                "partOfSpeech",
                "Unknown part of speech.");
        }
        var cefrLevel = ParseOptionalCefr(request.CefrLevel);
        var normalizedLemma = VocabularyTextNormalizer.NormalizeLemma(request.Lemma);
        var duplicateExists = await dbContext.Set<Lexeme>()
            .AnyAsync(
                lexeme =>
                    lexeme.OwnerUserId == userId
                    && lexeme.LanguageId == request.LanguageId
                    && lexeme.NormalizedLemma == normalizedLemma
                    && lexeme.PartOfSpeech == partOfSpeech,
                cancellationToken);
        if (duplicateExists)
        {
            return VocabularyResult<UserVocabularyItemResponse>.Conflict(
                "lemma",
                "You already created this private lexeme for the selected language and part of speech.");
        }

        var now = clock.UtcNow;
        var concept = Concept.CreatePrivate(
            userId,
            $"user.{userId:N}.{Guid.NewGuid():N}",
            now);
        var lexeme = Lexeme.CreatePrivate(
            userId,
            request.LanguageId,
            request.Lemma,
            partOfSpeech,
            cefrLevel,
            request.Notes,
            now);
        var sense = LexemeSense.CreatePrivate(
            userId,
            lexeme.Id,
            concept.Id,
            request.DefinitionLanguageId,
            request.Definition,
            cefrLevel,
            now);
        var item = UserVocabularyItem.Create(userId, sense.Id, now);

        dbContext.AddRange(concept, lexeme, sense, item);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(
            exception,
            "IX_Lexemes_OwnerUserId_LanguageId_PartOfSpeech_NormalizedLemma"))
        {
            dbContext.ChangeTracker.Clear();
            return VocabularyResult<UserVocabularyItemResponse>.Conflict(
                "lemma",
                "You already created this private lexeme for the selected language and part of speech.");
        }

        return await GetItemResponseAsync(item.Id, userId, cancellationToken);
    }

    public async Task<VocabularyResult<UserVocabularyResponse>> GetUserVocabularyAsync(
        Guid userId,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var pagingError = ValidatePaging(page, pageSize);
        if (pagingError is not null)
        {
            return VocabularyResult<UserVocabularyResponse>.Validation("page", pagingError);
        }

        UserVocabularyStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!TryParseNamedEnum(status, out UserVocabularyStatus value))
            {
                return VocabularyResult<UserVocabularyResponse>.Validation(
                    "status",
                    "Unknown vocabulary status.");
            }

            parsedStatus = value;
        }

        var itemQuery = dbContext.Set<UserVocabularyItem>()
            .AsNoTracking()
            .Where(item => item.UserId == userId);
        if (parsedStatus is not null)
        {
            itemQuery = itemQuery.Where(item => item.Status == parsedStatus);
        }

        var totalCount = await itemQuery.CountAsync(cancellationToken);
        var databaseRows = await (
            from item in itemQuery
            join sense in dbContext.Set<LexemeSense>().AsNoTracking()
                on item.LexemeSenseId equals sense.Id
            join lexeme in dbContext.Set<Lexeme>().AsNoTracking()
                on sense.LexemeId equals lexeme.Id
            orderby item.StatusChangedAt descending, lexeme.NormalizedLemma
            select new { Item = item, Sense = sense, Lexeme = lexeme })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
        var rows = databaseRows
            .Select(row => new UserVocabularyRow(row.Item, row.Sense, row.Lexeme))
            .ToArray();
        var items = await MapUserRowsAsync(rows, cancellationToken);

        return VocabularyResult<UserVocabularyResponse>.Success(
            new UserVocabularyResponse(page, pageSize, totalCount, items));
    }

    public async Task<VocabularyResult<UserVocabularyItemResponse>> UpdateStatusAsync(
        Guid userId,
        Guid itemId,
        UpdateVocabularyStatusRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!TryParseNamedEnum(request.Status, out UserVocabularyStatus status))
        {
            return VocabularyResult<UserVocabularyItemResponse>.Validation(
                "status",
                "Unknown vocabulary status.");
        }

        var item = await dbContext.Set<UserVocabularyItem>()
            .SingleOrDefaultAsync(
                candidate => candidate.Id == itemId && candidate.UserId == userId,
                cancellationToken);
        if (item is null)
        {
            return VocabularyResult<UserVocabularyItemResponse>.NotFound();
        }

        item.ChangeStatus(status, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetItemResponseAsync(item.Id, userId, cancellationToken);
    }

    private IQueryable<Lexeme> VisibleLexemes(Guid? userId) =>
        dbContext.Set<Lexeme>().Where(lexeme =>
            (lexeme.Visibility == VocabularyVisibility.Catalog
             && lexeme.PublicationStatus == PublicationStatus.Published)
            || (userId.HasValue
                && lexeme.Visibility == VocabularyVisibility.Private
                && lexeme.OwnerUserId == userId));

    private IQueryable<LexemeSense> VisibleSenses(Guid? userId) =>
        dbContext.Set<LexemeSense>().Where(sense =>
            (sense.Visibility == VocabularyVisibility.Catalog
             && sense.PublicationStatus == PublicationStatus.Published)
            || (userId.HasValue
                && sense.Visibility == VocabularyVisibility.Private
                && sense.OwnerUserId == userId));

    private IQueryable<ExampleSentence> VisibleExamples(Guid? userId) =>
        dbContext.Set<ExampleSentence>().Where(example =>
            (example.Visibility == VocabularyVisibility.Catalog
             && example.PublicationStatus == PublicationStatus.Published)
            || (userId.HasValue
                && example.Visibility == VocabularyVisibility.Private
                && example.OwnerUserId == userId));

    private async Task<VocabularyResult<UserVocabularyItemResponse>> GetItemResponseAsync(
        Guid itemId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var databaseRow = await (
            from item in dbContext.Set<UserVocabularyItem>().AsNoTracking()
            join sense in dbContext.Set<LexemeSense>().AsNoTracking()
                on item.LexemeSenseId equals sense.Id
            join lexeme in dbContext.Set<Lexeme>().AsNoTracking()
                on sense.LexemeId equals lexeme.Id
            where item.UserId == userId && item.Id == itemId
            select new { Item = item, Sense = sense, Lexeme = lexeme })
            .SingleOrDefaultAsync(cancellationToken);
        if (databaseRow is null)
        {
            return VocabularyResult<UserVocabularyItemResponse>.NotFound();
        }

        var row = new UserVocabularyRow(
            databaseRow.Item,
            databaseRow.Sense,
            databaseRow.Lexeme);
        var responses = await MapUserRowsAsync([row], cancellationToken);
        return VocabularyResult<UserVocabularyItemResponse>.Success(responses[0]);
    }

    private async Task<IReadOnlyList<UserVocabularyItemResponse>> MapUserRowsAsync(
        IReadOnlyCollection<UserVocabularyRow> rows,
        CancellationToken cancellationToken)
    {
        var languages = await languageCatalog.GetActiveByIdsAsync(
            rows.Select(row => row.Lexeme.LanguageId).Distinct().ToArray(),
            cancellationToken);

        return rows.Select(row =>
        {
            languages.TryGetValue(row.Lexeme.LanguageId, out var language);
            return new UserVocabularyItemResponse(
                row.Item.Id,
                row.Sense.Id,
                row.Lexeme.Id,
                row.Lexeme.Lemma,
                row.Lexeme.LanguageId,
                language?.Code ?? "unknown",
                language?.Name ?? "Unknown language",
                row.Lexeme.PartOfSpeech.ToString(),
                row.Sense.Definition,
                row.Item.Status.ToString(),
                row.Lexeme.Visibility == VocabularyVisibility.Private,
                row.Item.AddedAt,
                row.Item.StatusChangedAt);
        }).ToArray();
    }

    private static (string Key, string Message)? ValidatePrivateRequest(
        CreatePrivateVocabularyRequest request)
    {
        if (request.LanguageId == Guid.Empty || request.DefinitionLanguageId == Guid.Empty)
        {
            return ("languageId", "Select languages for the word and definition.");
        }

        if (string.IsNullOrWhiteSpace(request.Lemma) || request.Lemma.Trim().Length > 200)
        {
            return ("lemma", "Lemma must contain between 1 and 200 characters.");
        }

        if (!TryParseNamedEnum(request.PartOfSpeech, out PartOfSpeech _))
        {
            return ("partOfSpeech", "Unknown part of speech.");
        }

        if (!string.IsNullOrWhiteSpace(request.CefrLevel)
            && !TryParseNamedEnum(request.CefrLevel, out CefrLevel _))
        {
            return ("cefrLevel", "Unknown CEFR level.");
        }

        if (request.Notes?.Length > 2000)
        {
            return ("notes", "Notes cannot exceed 2000 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.Definition)
            || request.Definition.Trim().Length > 1000)
        {
            return ("definition", "Definition must contain between 1 and 1000 characters.");
        }

        return null;
    }

    private static CefrLevel? ParseOptionalCefr(string? value) =>
        string.IsNullOrWhiteSpace(value) || !TryParseNamedEnum(value, out CefrLevel level)
            ? null
            : level;

    private static bool TryParseNamedEnum<TEnum>(string? value, out TEnum result)
        where TEnum : struct, Enum
    {
        result = default;
        return !string.IsNullOrWhiteSpace(value)
            && Enum.GetNames<TEnum>().Contains(value.Trim(), StringComparer.OrdinalIgnoreCase)
            && Enum.TryParse(value.Trim(), ignoreCase: true, out result);
    }

    private static string? ValidatePaging(int page, int pageSize)
    {
        if (page < 1)
        {
            return "Page must be greater than zero.";
        }

        if (page > 100_000)
        {
            return "Page cannot be greater than 100000.";
        }

        return pageSize is < 1 or > 50
            ? "Page size must contain between 1 and 50 items."
            : null;
    }

    private static string GetLanguageCode(
        IReadOnlyDictionary<Guid, LanguageDescriptor> languages,
        Guid languageId) =>
        languages.GetValueOrDefault(languageId)?.Code ?? "unknown";

    private static bool IsUniqueViolation(
        DbUpdateException exception,
        string constraintName) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: var actualConstraint,
        }
        && string.Equals(actualConstraint, constraintName, StringComparison.Ordinal);

    private sealed record UserVocabularyRow(
        UserVocabularyItem Item,
        LexemeSense Sense,
        Lexeme Lexeme);
}
