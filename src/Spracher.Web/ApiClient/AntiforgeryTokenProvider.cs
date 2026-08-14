using System.Net.Http.Json;
using Spracher.Contracts.Identity;

namespace Spracher.Web.ApiClient;

public sealed class AntiforgeryTokenProvider(HttpClient httpClient)
{
    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<AntiforgeryTokenResponse>(
            "api/v1/auth/antiforgery",
            cancellationToken);

        return response?.Token
            ?? throw new InvalidOperationException(
                "The API returned an empty antiforgery token response.");
    }
}
