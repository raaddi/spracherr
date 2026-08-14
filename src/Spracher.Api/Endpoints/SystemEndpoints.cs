using Spracher.BuildingBlocks.Time;
using Spracher.Contracts.System;

namespace Spracher.Api.Endpoints;

internal static class SystemEndpoints
{
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints
            .MapGroup("/api/v1/system")
            .WithTags("System");

        group.MapGet(
                "/info",
                (IHostEnvironment environment, IClock clock) =>
                    TypedResults.Ok(new SystemInfoResponse(
                        Name: "Spracher API",
                        ApiVersion: "v1",
                        Environment: environment.EnvironmentName,
                        ServerTimeUtc: clock.UtcNow)))
            .WithName("GetSystemInfo")
            .WithSummary("Returns non-sensitive API runtime information.");

        return endpoints;
    }
}
