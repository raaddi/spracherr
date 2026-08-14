using System.Net.Http.Json;
using System.Text.Json;

namespace Spracher.Web.ApiClient;

public sealed class JsonApiClient(
    HttpClient httpClient,
    AntiforgeryTokenProvider antiforgeryTokenProvider)
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    public async Task<ApiResult<TResponse>> GetAsync<TResponse>(
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        return await ReadAsync<TResponse>(response, cancellationToken);
    }

    public async Task<ApiResult<TResponse>> SendAsync<TRequest, TResponse>(
        HttpMethod method,
        string requestUri,
        TRequest requestBody,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(method, requestUri)
        {
            Content = JsonContent.Create(requestBody, options: SerializerOptions),
        };
        request.Headers.Add(
            "X-XSRF-TOKEN",
            await antiforgeryTokenProvider.GetTokenAsync(cancellationToken));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        return await ReadAsync<TResponse>(response, cancellationToken);
    }

    public async Task<ApiResult<TResponse>> SendAsync<TResponse>(
        HttpMethod method,
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(method, requestUri);
        request.Headers.Add(
            "X-XSRF-TOKEN",
            await antiforgeryTokenProvider.GetTokenAsync(cancellationToken));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        return await ReadAsync<TResponse>(response, cancellationToken);
    }

    private static async Task<ApiResult<TResponse>> ReadAsync<TResponse>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var value = await response.Content.ReadFromJsonAsync<TResponse>(
                SerializerOptions,
                cancellationToken);
            return value is null
                ? ApiResult.Failure<TResponse>("API zwróciło pustą odpowiedź.")
                : ApiResult.Success(value);
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = $"Żądanie nie powiodło się (HTTP {(int)response.StatusCode}).";
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;
                if (root.TryGetProperty("detail", out var detail)
                    && detail.ValueKind == JsonValueKind.String)
                {
                    message = detail.GetString() ?? message;
                }
                else if (root.TryGetProperty("title", out var title)
                         && title.ValueKind == JsonValueKind.String)
                {
                    message = title.GetString() ?? message;
                }

                if (root.TryGetProperty("errors", out var errorObject)
                    && errorObject.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in errorObject.EnumerateObject())
                    {
                        errors[property.Name] = property.Value.ValueKind == JsonValueKind.Array
                            ? property.Value.EnumerateArray()
                                .Where(item => item.ValueKind == JsonValueKind.String)
                                .Select(item => item.GetString() ?? string.Empty)
                                .ToArray()
                            : [];
                    }
                }
            }
            catch (JsonException)
            {
                // Keep the status-based message when a proxy returns a non-JSON body.
            }
        }

        return ApiResult.Failure<TResponse>(message, errors);
    }
}
