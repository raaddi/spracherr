using Microsoft.EntityFrameworkCore;
using Npgsql;
using Spracher.BuildingBlocks.Languages;
using Spracher.BuildingBlocks.Time;
using Spracher.Contracts.Vocabulary;
using Spracher.Modules.Vocabulary.Domain;
using Spracher.Persistence;

namespace Spracher.Modules.Vocabulary.Application;

internal sealed class VocabularyCollectionService(
    SpracherDbContext dbContext,
    ILanguageCatalogReader languageCatalog,
    IClock clock)
{
    public async Task<VocabularyResult<VocabularyListsResponse>> GetListsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var items = await dbContext.Set<VocabularyList>()
            .AsNoTracking()
            .Where(list => list.OwnerUserId == userId)
            .OrderBy(list => list.NormalizedName)
            .Select(list => new VocabularyListSummaryResponse(
                list.Id,
                list.Name,
                list.Description,
                dbContext.Set<VocabularyListItem>()
                    .Count(item => item.VocabularyListId == list.Id),
                list.UpdatedAt))
            .ToArrayAsync(cancellationToken);

        return VocabularyResult<VocabularyListsResponse>.Success(
            new VocabularyListsResponse(items));
    }

    public async Task<VocabularyResult<VocabularyListDetailsResponse>> GetListAsync(
        Guid userId,
        Guid listId,
        CancellationToken cancellationToken)
    {
        var list = await dbContext.Set<VocabularyList>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.Id == listId && candidate.OwnerUserId == userId,
                cancellationToken);
        if (list is null)
        {
            return VocabularyResult<VocabularyListDetailsResponse>.NotFound();
        }

        return VocabularyResult<VocabularyListDetailsResponse>.Success(
            await MapListDetailsAsync(list, userId, cancellationToken));
    }

    public async Task<VocabularyResult<VocabularyListDetailsResponse>> CreateListAsync(
        Guid userId,
        CreateVocabularyListRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validation = ValidateList(request);
        if (validation is not null)
        {
            return VocabularyResult<VocabularyListDetailsResponse>.Validation(
                validation.Value.Key,
                validation.Value.Message);
        }

        var normalizedName = VocabularyTextNormalizer.NormalizeLemma(request.Name);
        var duplicateExists = await dbContext.Set<VocabularyList>()
            .AnyAsync(
                list => list.OwnerUserId == userId && list.NormalizedName == normalizedName,
                cancellationToken);
        if (duplicateExists)
        {
            return VocabularyResult<VocabularyListDetailsResponse>.Conflict(
                "name",
                "You already have a vocabulary list with this name.");
        }

        var list = VocabularyList.Create(userId, request.Name, request.Description, clock.UtcNow);
        dbContext.Add(list);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(
            exception,
            "IX_VocabularyLists_OwnerUserId_NormalizedName"))
        {
            dbContext.ChangeTracker.Clear();
            return VocabularyResult<VocabularyListDetailsResponse>.Conflict(
                "name",
                "You already have a vocabulary list with this name.");
        }

        return VocabularyResult<VocabularyListDetailsResponse>.Success(
            new VocabularyListDetailsResponse(
                list.Id,
                list.Name,
                list.Description,
                [],
                list.UpdatedAt));
    }

    public async Task<VocabularyResult<VocabularyListDetailsResponse>> AddToListAsync(
        Guid userId,
        Guid listId,
        AddVocabularyListItemRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.UserVocabularyItemId == Guid.Empty)
        {
            return VocabularyResult<VocabularyListDetailsResponse>.Validation(
                "userVocabularyItemId",
                "Select a vocabulary item.");
        }

        if (request.Note?.Length > 500)
        {
            return VocabularyResult<VocabularyListDetailsResponse>.Validation(
                "note",
                "List item note cannot exceed 500 characters.");
        }

        var list = await dbContext.Set<VocabularyList>()
            .SingleOrDefaultAsync(
                candidate => candidate.Id == listId && candidate.OwnerUserId == userId,
                cancellationToken);
        if (list is null)
        {
            return VocabularyResult<VocabularyListDetailsResponse>.NotFound();
        }

        var userItem = await dbContext.Set<UserVocabularyItem>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == request.UserVocabularyItemId && item.UserId == userId,
                cancellationToken);
        if (userItem is null)
        {
            return VocabularyResult<VocabularyListDetailsResponse>.NotFound();
        }

        var existing = await dbContext.Set<VocabularyListItem>()
            .AsNoTracking()
            .AnyAsync(
                item => item.VocabularyListId == listId
                        && item.LexemeSenseId == userItem.LexemeSenseId,
                cancellationToken);
        if (!existing)
        {
            var lastPosition = await dbContext.Set<VocabularyListItem>()
                .Where(item => item.VocabularyListId == listId)
                .Select(item => (int?)item.Position)
                .MaxAsync(cancellationToken);
            var now = clock.UtcNow;
            dbContext.Add(VocabularyListItem.Create(
                listId,
                userItem.LexemeSenseId,
                (lastPosition ?? -1) + 1,
                request.Note,
                now));
            list.Touch(now);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (IsUniqueViolation(
                exception,
                "PK_VocabularyListItems"))
            {
                dbContext.ChangeTracker.Clear();
            }
        }

        return VocabularyResult<VocabularyListDetailsResponse>.Success(
            await MapListDetailsAsync(list, userId, cancellationToken));
    }

    public async Task<VocabularyResult<VocabularyMutationResponse>> RemoveFromListAsync(
        Guid userId,
        Guid listId,
        Guid userVocabularyItemId,
        CancellationToken cancellationToken)
    {
        var list = await dbContext.Set<VocabularyList>()
            .SingleOrDefaultAsync(
                candidate => candidate.Id == listId && candidate.OwnerUserId == userId,
                cancellationToken);
        if (list is null)
        {
            return VocabularyResult<VocabularyMutationResponse>.NotFound();
        }

        var senseId = await dbContext.Set<UserVocabularyItem>()
            .AsNoTracking()
            .Where(item => item.Id == userVocabularyItemId && item.UserId == userId)
            .Select(item => (Guid?)item.LexemeSenseId)
            .SingleOrDefaultAsync(cancellationToken);
        if (senseId is null)
        {
            return VocabularyResult<VocabularyMutationResponse>.NotFound();
        }

        var listItem = await dbContext.Set<VocabularyListItem>()
            .SingleOrDefaultAsync(
                item => item.VocabularyListId == listId
                        && item.LexemeSenseId == senseId.Value,
                cancellationToken);
        if (listItem is null)
        {
            return VocabularyResult<VocabularyMutationResponse>.NotFound();
        }

        dbContext.Remove(listItem);
        list.Touch(clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return VocabularyResult<VocabularyMutationResponse>.Success(
            new VocabularyMutationResponse("Vocabulary item removed from the list."));
    }

    public async Task<VocabularyResult<VocabularyCategoriesResponse>> GetCategoriesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var categories = await dbContext.Set<VocabularyCategory>()
            .AsNoTracking()
            .Where(category => category.OwnerUserId == userId)
            .OrderBy(category => category.NormalizedName)
            .ToArrayAsync(cancellationToken);
        var categoryIds = categories.Select(category => category.Id).ToArray();
        var assignments = categoryIds.Length == 0
            ? []
            : await dbContext.Set<UserVocabularyItemCategory>()
                .AsNoTracking()
                .Where(assignment => categoryIds.Contains(assignment.VocabularyCategoryId))
                .ToArrayAsync(cancellationToken);

        return VocabularyResult<VocabularyCategoriesResponse>.Success(
            new VocabularyCategoriesResponse(categories.Select(category =>
                new VocabularyCategoryResponse(
                    category.Id,
                    category.Name,
                    category.Color,
                    assignments
                        .Where(assignment => assignment.VocabularyCategoryId == category.Id)
                        .Select(assignment => assignment.UserVocabularyItemId)
                        .ToArray())).ToArray()));
    }

    public async Task<VocabularyResult<VocabularyCategoryResponse>> CreateCategoryAsync(
        Guid userId,
        CreateVocabularyCategoryRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validation = ValidateCategory(request);
        if (validation is not null)
        {
            return VocabularyResult<VocabularyCategoryResponse>.Validation(
                validation.Value.Key,
                validation.Value.Message);
        }

        var normalizedName = VocabularyTextNormalizer.NormalizeLemma(request.Name);
        var duplicateExists = await dbContext.Set<VocabularyCategory>()
            .AnyAsync(
                category => category.OwnerUserId == userId
                            && category.NormalizedName == normalizedName,
                cancellationToken);
        if (duplicateExists)
        {
            return VocabularyResult<VocabularyCategoryResponse>.Conflict(
                "name",
                "You already have a vocabulary category with this name.");
        }

        var category = VocabularyCategory.Create(
            userId,
            request.Name,
            request.Color,
            clock.UtcNow);
        dbContext.Add(category);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(
            exception,
            "IX_VocabularyCategories_OwnerUserId_NormalizedName"))
        {
            dbContext.ChangeTracker.Clear();
            return VocabularyResult<VocabularyCategoryResponse>.Conflict(
                "name",
                "You already have a vocabulary category with this name.");
        }

        return VocabularyResult<VocabularyCategoryResponse>.Success(
            new VocabularyCategoryResponse(
                category.Id,
                category.Name,
                category.Color,
                []));
    }

    public async Task<VocabularyResult<UserVocabularyCategoriesResponse>> AssignCategoriesAsync(
        Guid userId,
        Guid userVocabularyItemId,
        AssignVocabularyCategoriesRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var categoryIds = request.CategoryIds.Distinct().ToArray();
        if (categoryIds.Length > 20 || categoryIds.Any(id => id == Guid.Empty))
        {
            return VocabularyResult<UserVocabularyCategoriesResponse>.Validation(
                "categoryIds",
                "Select at most 20 valid categories.");
        }

        var itemExists = await dbContext.Set<UserVocabularyItem>()
            .AsNoTracking()
            .AnyAsync(
                item => item.Id == userVocabularyItemId && item.UserId == userId,
                cancellationToken);
        if (!itemExists)
        {
            return VocabularyResult<UserVocabularyCategoriesResponse>.NotFound();
        }

        var ownedCategoryCount = await dbContext.Set<VocabularyCategory>()
            .CountAsync(
                category => category.OwnerUserId == userId
                            && categoryIds.Contains(category.Id),
                cancellationToken);
        if (ownedCategoryCount != categoryIds.Length)
        {
            return VocabularyResult<UserVocabularyCategoriesResponse>.Validation(
                "categoryIds",
                "Every category must belong to the current user.");
        }

        var current = await dbContext.Set<UserVocabularyItemCategory>()
            .Where(assignment => assignment.UserVocabularyItemId == userVocabularyItemId)
            .ToArrayAsync(cancellationToken);
        var requestedIds = categoryIds.ToHashSet();
        dbContext.RemoveRange(current.Where(assignment =>
            !requestedIds.Contains(assignment.VocabularyCategoryId)));
        var currentIds = current.Select(assignment => assignment.VocabularyCategoryId).ToHashSet();
        dbContext.AddRange(categoryIds
            .Where(categoryId => !currentIds.Contains(categoryId))
            .Select(categoryId => UserVocabularyItemCategory.Create(
                userVocabularyItemId,
                categoryId,
                clock.UtcNow)));
        await dbContext.SaveChangesAsync(cancellationToken);

        return VocabularyResult<UserVocabularyCategoriesResponse>.Success(
            new UserVocabularyCategoriesResponse(userVocabularyItemId, categoryIds));
    }

    private async Task<VocabularyListDetailsResponse> MapListDetailsAsync(
        VocabularyList list,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var rows = await (
            from listItem in dbContext.Set<VocabularyListItem>().AsNoTracking()
            join sense in dbContext.Set<LexemeSense>().AsNoTracking()
                on listItem.LexemeSenseId equals sense.Id
            join lexeme in dbContext.Set<Lexeme>().AsNoTracking()
                on sense.LexemeId equals lexeme.Id
            join userItem in dbContext.Set<UserVocabularyItem>().AsNoTracking()
                on sense.Id equals userItem.LexemeSenseId
            where listItem.VocabularyListId == list.Id && userItem.UserId == userId
            orderby listItem.Position
            select new { ListItem = listItem, Sense = sense, Lexeme = lexeme, UserItem = userItem })
            .ToArrayAsync(cancellationToken);
        var languages = await languageCatalog.GetActiveByIdsAsync(
            rows.Select(row => row.Lexeme.LanguageId).Distinct().ToArray(),
            cancellationToken);

        var items = rows.Select(row => new VocabularyListItemResponse(
            row.UserItem.Id,
            row.Sense.Id,
            row.Lexeme.Id,
            row.Lexeme.Lemma,
            languages.GetValueOrDefault(row.Lexeme.LanguageId)?.Code ?? "unknown",
            row.UserItem.Status.ToString(),
            row.ListItem.Position,
            row.ListItem.Note,
            row.ListItem.AddedAt)).ToArray();

        return new VocabularyListDetailsResponse(
            list.Id,
            list.Name,
            list.Description,
            items,
            list.UpdatedAt);
    }

    private static (string Key, string Message)? ValidateList(CreateVocabularyListRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 100)
        {
            return ("name", "List name must contain between 1 and 100 characters.");
        }

        return request.Description?.Length > 500
            ? ("description", "List description cannot exceed 500 characters.")
            : null;
    }

    private static (string Key, string Message)? ValidateCategory(
        CreateVocabularyCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 60)
        {
            return ("name", "Category name must contain between 1 and 60 characters.");
        }

        if (!VocabularyCategory.IsValidColor(request.Color))
        {
            return ("color", "Category color must use #RRGGBB format.");
        }

        return null;
    }

    private static bool IsUniqueViolation(
        DbUpdateException exception,
        string constraintName) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: var actualConstraint,
        }
        && string.Equals(actualConstraint, constraintName, StringComparison.Ordinal);
}
