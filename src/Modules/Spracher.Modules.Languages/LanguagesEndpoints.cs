using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Spracher.Contracts.Languages;
using Spracher.Modules.Languages.Application;

namespace Spracher.Modules.Languages;

public static class LanguagesEndpoints
{
    public static IEndpointRouteBuilder MapLanguagesEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/v1/languages").WithTags("Languages");
        group.MapGet("", GetCatalog).WithName("GetLanguages");
        group.MapGet("/me", GetUserLanguages)
            .RequireAuthorization()
            .WithName("GetUserLanguages");
        group.MapPut("/me", UpdateUserLanguages)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .WithName("UpdateUserLanguages");

        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<LanguageResponse>>> GetCatalog(
        LanguageProfileService service,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await service.GetCatalogAsync(cancellationToken));

    private static async Task<IResult> GetUserLanguages(
        ClaimsPrincipal principal,
        LanguageProfileService service,
        CancellationToken cancellationToken)
    {
        return TryGetUserId(principal, out var userId)
            ? Results.Ok(await service.GetUserLanguagesAsync(userId, cancellationToken))
            : Results.Unauthorized();
    }

    private static async Task<IResult> UpdateUserLanguages(
        ClaimsPrincipal principal,
        UpdateUserLanguagesRequest request,
        LanguageProfileService service,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return Results.Unauthorized();
        }

        var result = await service.UpdateUserLanguagesAsync(
            userId,
            request,
            cancellationToken);
        return result.Succeeded
            ? Results.Ok(result.Languages)
            : Results.ValidationProblem(result.Errors.ToDictionary(
                pair => pair.Key,
                pair => pair.Value,
                StringComparer.Ordinal));
    }

    private static bool TryGetUserId(ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
