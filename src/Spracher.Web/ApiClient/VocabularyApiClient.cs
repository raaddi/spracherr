using Spracher.Contracts.Vocabulary;

namespace Spracher.Web.ApiClient;

public sealed class VocabularyApiClient(JsonApiClient apiClient)
{
    public Task<ApiResult<VocabularySearchResponse>> SearchAsync(
        Guid languageId,
        string? query,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var encodedQuery = Uri.EscapeDataString(query?.Trim() ?? string.Empty);
        var requestUri =
            $"api/v1/vocabulary/search?languageId={languageId:D}"
            + $"&query={encodedQuery}&page={page}&pageSize={pageSize}";
        return apiClient.GetAsync<VocabularySearchResponse>(requestUri, cancellationToken);
    }

    public Task<ApiResult<VocabularyDetailsResponse>> GetDetailsAsync(
        Guid lexemeId,
        CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<VocabularyDetailsResponse>(
            $"api/v1/vocabulary/lexemes/{lexemeId:D}",
            cancellationToken);

    public Task<ApiResult<UserVocabularyResponse>> GetUserVocabularyAsync(
        string? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var encodedStatus = Uri.EscapeDataString(status?.Trim() ?? string.Empty);
        var requestUri =
            $"api/v1/vocabulary/me?status={encodedStatus}&page={page}&pageSize={pageSize}";
        return apiClient.GetAsync<UserVocabularyResponse>(requestUri, cancellationToken);
    }

    public Task<ApiResult<UserVocabularyItemResponse>> AddItemAsync(
        Guid lexemeSenseId,
        CancellationToken cancellationToken = default) =>
        apiClient.SendAsync<AddVocabularyItemRequest, UserVocabularyItemResponse>(
            HttpMethod.Post,
            "api/v1/vocabulary/items",
            new AddVocabularyItemRequest(lexemeSenseId),
            cancellationToken);

    public Task<ApiResult<UserVocabularyItemResponse>> CreatePrivateAsync(
        CreatePrivateVocabularyRequest request,
        CancellationToken cancellationToken = default) =>
        apiClient.SendAsync<CreatePrivateVocabularyRequest, UserVocabularyItemResponse>(
            HttpMethod.Post,
            "api/v1/vocabulary/private",
            request,
            cancellationToken);

    public Task<ApiResult<UserVocabularyItemResponse>> UpdateStatusAsync(
        Guid itemId,
        string status,
        CancellationToken cancellationToken = default) =>
        apiClient.SendAsync<UpdateVocabularyStatusRequest, UserVocabularyItemResponse>(
            HttpMethod.Put,
            $"api/v1/vocabulary/items/{itemId:D}/status",
            new UpdateVocabularyStatusRequest(status),
            cancellationToken);

    public Task<ApiResult<VocabularyListsResponse>> GetListsAsync(
        CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<VocabularyListsResponse>(
            "api/v1/vocabulary/me/lists",
            cancellationToken);

    public Task<ApiResult<VocabularyListDetailsResponse>> GetListAsync(
        Guid listId,
        CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<VocabularyListDetailsResponse>(
            $"api/v1/vocabulary/lists/{listId:D}",
            cancellationToken);

    public Task<ApiResult<VocabularyListDetailsResponse>> CreateListAsync(
        CreateVocabularyListRequest request,
        CancellationToken cancellationToken = default) =>
        apiClient.SendAsync<CreateVocabularyListRequest, VocabularyListDetailsResponse>(
            HttpMethod.Post,
            "api/v1/vocabulary/lists",
            request,
            cancellationToken);

    public Task<ApiResult<VocabularyListDetailsResponse>> AddToListAsync(
        Guid listId,
        AddVocabularyListItemRequest request,
        CancellationToken cancellationToken = default) =>
        apiClient.SendAsync<AddVocabularyListItemRequest, VocabularyListDetailsResponse>(
            HttpMethod.Post,
            $"api/v1/vocabulary/lists/{listId:D}/items",
            request,
            cancellationToken);

    public Task<ApiResult<VocabularyMutationResponse>> RemoveFromListAsync(
        Guid listId,
        Guid userVocabularyItemId,
        CancellationToken cancellationToken = default) =>
        apiClient.SendAsync<VocabularyMutationResponse>(
            HttpMethod.Delete,
            $"api/v1/vocabulary/lists/{listId:D}/items/{userVocabularyItemId:D}",
            cancellationToken);

    public Task<ApiResult<VocabularyCategoriesResponse>> GetCategoriesAsync(
        CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<VocabularyCategoriesResponse>(
            "api/v1/vocabulary/me/categories",
            cancellationToken);

    public Task<ApiResult<VocabularyCategoryResponse>> CreateCategoryAsync(
        CreateVocabularyCategoryRequest request,
        CancellationToken cancellationToken = default) =>
        apiClient.SendAsync<CreateVocabularyCategoryRequest, VocabularyCategoryResponse>(
            HttpMethod.Post,
            "api/v1/vocabulary/categories",
            request,
            cancellationToken);

    public Task<ApiResult<UserVocabularyCategoriesResponse>> AssignCategoriesAsync(
        Guid userVocabularyItemId,
        IReadOnlyList<Guid> categoryIds,
        CancellationToken cancellationToken = default) =>
        apiClient.SendAsync<AssignVocabularyCategoriesRequest, UserVocabularyCategoriesResponse>(
            HttpMethod.Put,
            $"api/v1/vocabulary/items/{userVocabularyItemId:D}/categories",
            new AssignVocabularyCategoriesRequest(categoryIds),
            cancellationToken);
}
