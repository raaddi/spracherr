using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Spracher.Persistence;

public static class DatabaseMigrationExtensions
{
    public static async Task ApplySpracherMigrationsAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SpracherDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Spracher.DatabaseMigration");

        var pendingMigrations = await dbContext.Database
            .GetPendingMigrationsAsync(cancellationToken);
        var pendingMigrationNames = pendingMigrations.ToArray();

        DatabaseMigrationLog.ApplyingMigrations(logger, pendingMigrationNames.Length);

        await dbContext.Database.MigrateAsync(cancellationToken);

        DatabaseMigrationLog.MigrationsCompleted(logger);
    }
}

internal static partial class DatabaseMigrationLog
{
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Applying {MigrationCount} pending database migration(s)")]
    public static partial void ApplyingMigrations(ILogger logger, int migrationCount);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Database migrations completed successfully")]
    public static partial void MigrationsCompleted(ILogger logger);
}
