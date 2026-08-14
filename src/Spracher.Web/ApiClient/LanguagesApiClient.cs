using Spracher.Contracts.Languages;

namespace Spracher.Web.ApiClient;

public sealed class LanguagesApiClient(JsonApiClient apiClient)
{
    public Task<ApiResult<IReadOnlyList<LanguageResponse>>> GetCatalogAsync(
        CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<IReadOnlyList<LanguageResponse>>(
            "api/v1/languages",
            cancellationToken);

    public Task<ApiResult<IReadOnlyList<UserLanguageProfileResponse>>> GetUserLanguagesAsync(
        CancellationToken cancellationToken = default) =>
        apiClient.GetAsync<IReadOnlyList<UserLanguageProfileResponse>>(
            "api/v1/languages/me",
            cancellationToken);

    public Task<ApiResult<IReadOnlyList<UserLanguageProfileResponse>>> UpdateUserLanguagesAsync(
        UpdateUserLanguagesRequest request,
        CancellationToken cancellationToken = default) =>
        apiClient.SendAsync<UpdateUserLanguagesRequest, IReadOnlyList<UserLanguageProfileResponse>>(
            HttpMethod.Put,
            "api/v1/languages/me",
            request,
            cancellationToken);
}
