using System.Net;
using System.Net.Http.Json;
using Spracher.Api.IntegrationTests.Infrastructure;
using Spracher.Contracts.System;

namespace Spracher.Api.IntegrationTests;

public sealed class SystemEndpointsTests(PostgresWebApplicationFactory factory)
    : IClassFixture<PostgresWebApplicationFactory>
{
    [IntegrationFact]
    public async Task SystemInfoShouldReturnApiMetadata()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/api/v1/system/info", UriKind.Relative));
        var systemInfo = await response.Content.ReadFromJsonAsync<SystemInfoResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(systemInfo);
        Assert.Equal("Spracher API", systemInfo.Name);
        Assert.Equal("v1", systemInfo.ApiVersion);
        Assert.Equal("Testing", systemInfo.Environment);
    }

    [IntegrationFact]
    public async Task ReadinessShouldUsePostgreSqlHealthCheck()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/health/ready", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [IntegrationFact]
    public async Task UnknownEndpointShouldReturnProblemDetails()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Accept.ParseAdd("application/problem+json");

        var response = await client.GetAsync(new Uri("/api/v1/does-not-exist", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
