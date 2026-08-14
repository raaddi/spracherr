using System.Net.Http.Json;
using Spracher.Contracts.System;

namespace Spracher.Web.ApiClient;

public sealed class SystemApiClient(HttpClient httpClient)
{
    public async Task<SystemInfoResponse> GetSystemInfoAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<SystemInfoResponse>(
            "api/v1/system/info",
            cancellationToken);

        return response ?? throw new InvalidOperationException(
            "The API returned an empty system information response.");
    }
}
