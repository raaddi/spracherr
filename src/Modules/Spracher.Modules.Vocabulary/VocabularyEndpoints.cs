using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Spracher.Contracts.Vocabulary;
using Spracher.Modules.Vocabulary.Application;

namespace Spracher.Modules.Vocabulary;

public static class VocabularyEndpoints
{
    public static IEndpointRouteBuilder MapVocabularyEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/v1/vocabulary").WithTags("Vocabulary");
        group.MapGet("/search", Search).WithName("SearchVocabulary");
        group.MapGet("/lexemes/{lexemeId:guid}", GetDetails)
            .WithName("GetVocabularyDetails");

        group.MapGet("/me", GetUserVocabulary)
            .RequireAuthorization()
            .WithName("GetUserVocabulary");
        group.MapPost("/items", AddItem)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .WithName("AddUserVocabularyItem");
        group.MapPut("/items/{itemId:guid}/status", UpdateStatus)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .WithName("UpdateUserVocabularyStatus");
        group.MapPost("/private", CreatePrivate)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .WithName("CreatePrivateVocabulary");

        group.MapGet("/me/lists", GetLists)
            .RequireAuthorization()
            .WithName("GetVocabularyLists");
        group.MapGet("/lists/{listId:guid}", GetList)
            .RequireAuthorization()
            .WithName("GetVocabularyList");
        group.MapPost("/lists", CreateList)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .WithName("CreateVocabularyList");
        group.MapPost("/lists/{listId:guid}/items", AddToList)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .WithName("AddVocabularyListItem");
        group.MapDelete("/lists/{listId:guid}/items/{userVocabularyItemId:guid}", RemoveFromList)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .WithName("RemoveVocabularyListItem");
        group.MapGet("/me/categories", GetCategories)
            .RequireAuthorization()
            .WithName("GetVocabularyCategories");
        group.MapPost("/categories", CreateCategory)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .WithName("CreateVocabularyCategory");
        group.MapPut("/items/{itemId:guid}/categories", AssignCategories)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .WithName("AssignVocabularyCategories");

        return endpoints;
    }

    private static async Task<IResult> Search(
        Guid languageId,
        ClaimsPrincipal principal,
        VocabularyService service,
        CancellationToken cancellationToken,
        string? query = null,
        int page = 1,
        int pageSize = 20) =>
        MapResult(
            await service.SearchAsync(
                GetOptionalUserId(principal),
                languageId,
                query,
                page,
                pageSize,
                cancellationToken),
            Results.Ok);

    private static async Task<IResult> GetDetails(
        Guid lexemeId,
        ClaimsPrincipal principal,
        VocabularyService service,
        CancellationToken cancellationToken) =>
        MapResult(
            await service.GetDetailsAsync(
                GetOptionalUserId(principal),
                lexemeId,
                cancellationToken),
            Results.Ok);

    private static async Task<IResult> GetUserVocabulary(
        ClaimsPrincipal principal,
        VocabularyService service,
        CancellationToken cancellationToken,
        string? status = null,
        int page = 1,
        int pageSize = 20)
    {
        return TryGetUserId(principal, out var userId)
            ? MapResult(
                await service.GetUserVocabularyAsync(
                    userId,
                    status,
                    page,
                    pageSize,
                    cancellationToken),
                Results.Ok)
            : Results.Unauthorized();
    }

    private static async Task<IResult> AddItem(
        ClaimsPrincipal principal,
        AddVocabularyItemRequest request,
        VocabularyService service,
        CancellationToken cancellationToken)
    {
        return TryGetUserId(principal, out var userId)
            ? MapResult(
                await service.AddItemAsync(userId, request, cancellationToken),
                Results.Ok)
            : Results.Unauthorized();
    }

    private static async Task<IResult> CreatePrivate(
        ClaimsPrincipal principal,
        CreatePrivateVocabularyRequest request,
        VocabularyService service,
        CancellationToken cancellationToken)
    {
        return TryGetUserId(principal, out var userId)
            ? MapResult(
                await service.CreatePrivateAsync(userId, request, cancellationToken),
                value => Results.Created("/api/v1/vocabulary/me", value))
            : Results.Unauthorized();
    }

    private static async Task<IResult> UpdateStatus(
        Guid itemId,
        ClaimsPrincipal principal,
        UpdateVocabularyStatusRequest request,
        VocabularyService service,
        CancellationToken cancellationToken)
    {
        return TryGetUserId(principal, out var userId)
            ? MapResult(
                await service.UpdateStatusAsync(
                    userId,
                    itemId,
                    request,
                    cancellationToken),
                Results.Ok)
            : Results.Unauthorized();
    }

    private static async Task<IResult> GetLists(
        ClaimsPrincipal principal,
        VocabularyCollectionService service,
        CancellationToken cancellationToken) =>
        TryGetUserId(principal, out var userId)
            ? MapResult(await service.GetListsAsync(userId, cancellationToken), Results.Ok)
            : Results.Unauthorized();

    private static async Task<IResult> GetList(
        Guid listId,
        ClaimsPrincipal principal,
        VocabularyCollectionService service,
        CancellationToken cancellationToken) =>
        TryGetUserId(principal, out var userId)
            ? MapResult(
                await service.GetListAsync(userId, listId, cancellationToken),
                Results.Ok)
            : Results.Unauthorized();

    private static async Task<IResult> CreateList(
        ClaimsPrincipal principal,
        CreateVocabularyListRequest request,
        VocabularyCollectionService service,
        CancellationToken cancellationToken) =>
        TryGetUserId(principal, out var userId)
            ? MapResult(
                await service.CreateListAsync(userId, request, cancellationToken),
                value => Results.Created($"/api/v1/vocabulary/lists/{value.Id}", value))
            : Results.Unauthorized();

    private static async Task<IResult> AddToList(
        Guid listId,
        ClaimsPrincipal principal,
        AddVocabularyListItemRequest request,
        VocabularyCollectionService service,
        CancellationToken cancellationToken) =>
        TryGetUserId(principal, out var userId)
            ? MapResult(
                await service.AddToListAsync(userId, listId, request, cancellationToken),
                Results.Ok)
            : Results.Unauthorized();

    private static async Task<IResult> RemoveFromList(
        Guid listId,
        Guid userVocabularyItemId,
        ClaimsPrincipal principal,
        VocabularyCollectionService service,
        CancellationToken cancellationToken) =>
        TryGetUserId(principal, out var userId)
            ? MapResult(
                await service.RemoveFromListAsync(
                    userId,
                    listId,
                    userVocabularyItemId,
                    cancellationToken),
                Results.Ok)
            : Results.Unauthorized();

    private static async Task<IResult> GetCategories(
        ClaimsPrincipal principal,
        VocabularyCollectionService service,
        CancellationToken cancellationToken) =>
        TryGetUserId(principal, out var userId)
            ? MapResult(
                await service.GetCategoriesAsync(userId, cancellationToken),
                Results.Ok)
            : Results.Unauthorized();

    private static async Task<IResult> CreateCategory(
        ClaimsPrincipal principal,
        CreateVocabularyCategoryRequest request,
        VocabularyCollectionService service,
        CancellationToken cancellationToken) =>
        TryGetUserId(principal, out var userId)
            ? MapResult(
                await service.CreateCategoryAsync(userId, request, cancellationToken),
                value => Results.Created("/api/v1/vocabulary/me/categories", value))
            : Results.Unauthorized();

    private static async Task<IResult> AssignCategories(
        Guid itemId,
        ClaimsPrincipal principal,
        AssignVocabularyCategoriesRequest request,
        VocabularyCollectionService service,
        CancellationToken cancellationToken) =>
        TryGetUserId(principal, out var userId)
            ? MapResult(
                await service.AssignCategoriesAsync(
                    userId,
                    itemId,
                    request,
                    cancellationToken),
                Results.Ok)
            : Results.Unauthorized();

    private static IResult MapResult<T>(
        VocabularyResult<T> result,
        Func<T, IResult> success)
    {
        if (result.Kind == VocabularyResultKind.Success && result.Value is not null)
        {
            return success(result.Value);
        }

        var errors = result.Errors.ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal);

        return result.Kind switch
        {
            VocabularyResultKind.ValidationError => Results.ValidationProblem(errors),
            VocabularyResultKind.Conflict => Results.ValidationProblem(
                errors,
                statusCode: StatusCodes.Status409Conflict,
                title: "The vocabulary item already exists."),
            VocabularyResultKind.NotFound => Results.NotFound(),
            _ => Results.Problem(statusCode: StatusCodes.Status500InternalServerError),
        };
    }

    private static Guid? GetOptionalUserId(ClaimsPrincipal principal) =>
        TryGetUserId(principal, out var userId) ? userId : null;

    private static bool TryGetUserId(ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
