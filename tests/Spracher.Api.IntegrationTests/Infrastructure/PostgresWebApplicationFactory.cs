using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Spracher.Persistence;
using Testcontainers.PostgreSql;

namespace Spracher.Api.IntegrationTests.Infrastructure;

public sealed class PostgresWebApplicationFactory :
    WebApplicationFactory<Program>,
    IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("spracher_tests")
        .WithUsername("spracher_tests")
        .WithPassword("spracher_tests")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await Services.ApplySpracherMigrationsAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = _postgres.GetConnectionString(),
            });
        });
    }
}
