using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Spracher.Contracts.Exercises;
using Spracher.IdentityModel;
using Spracher.Modules.Exercises.Application;

namespace Spracher.Modules.Exercises;

public static class ExercisesEndpoints
{
    public static IEndpointRouteBuilder MapExercisesEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var exercises = endpoints.MapGroup("/api/v1/exercises").WithTags("Exercises");
        exercises.MapGet("/", GetCatalog).WithName("GetExerciseCatalog");
        exercises.MapPost("/{definitionId:guid}/attempts", StartAttempt)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .WithName("StartExerciseAttempt");

        endpoints.MapPost("/api/v1/exercise-attempts/{attemptId:guid}/submit", SubmitAttempt)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .WithTags("Exercises")
            .WithName("SubmitExerciseAttempt");

        var authoring = endpoints.MapGroup("/api/v1/exercise-authoring")
            .WithTags("Exercise authoring")
            .RequireAuthorization(policy => policy.RequireRole(SystemRoles.Admin));
        authoring.MapPost("/definitions", CreateDefinition)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .WithName("CreateExerciseDefinition");
        authoring.MapPost("/definitions/{definitionId:guid}/versions", CreateVersion)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .WithName("CreateExerciseVersion");
        authoring.MapPost("/versions/{versionId:guid}/publish", PublishVersion)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .WithName("PublishExerciseVersion");

        return endpoints;
    }

    private static async Task<IResult> GetCatalog(
        ExerciseService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.GetCatalogAsync(cancellationToken));

    private static async Task<IResult> StartAttempt(
        Guid definitionId,
        ClaimsPrincipal principal,
        ExerciseService service,
        CancellationToken cancellationToken) =>
        TryGetUserId(principal, out var userId)
            ? MapResult(
                await service.StartAttemptAsync(userId, definitionId, cancellationToken),
                Results.Ok)
            : Results.Unauthorized();

    private static async Task<IResult> SubmitAttempt(
        Guid attemptId,
        ClaimsPrincipal principal,
        SubmitExerciseAttemptRequest request,
        ExerciseService service,
        CancellationToken cancellationToken) =>
        TryGetUserId(principal, out var userId)
            ? MapResult(
                await service.SubmitAttemptAsync(
                    userId,
                    attemptId,
                    request,
                    cancellationToken),
                Results.Ok)
            : Results.Unauthorized();

    private static async Task<IResult> CreateDefinition(
        ClaimsPrincipal principal,
        CreateExerciseDefinitionRequest request,
        ExerciseAuthoringService service,
        CancellationToken cancellationToken) =>
        TryGetUserId(principal, out var userId)
            ? MapResult(
                await service.CreateDefinitionAsync(userId, request, cancellationToken),
                value => Results.Created(
                    $"/api/v1/exercise-authoring/versions/{value.ExerciseVersionId}",
                    value))
            : Results.Unauthorized();

    private static async Task<IResult> CreateVersion(
        Guid definitionId,
        CreateExerciseVersionRequest request,
        ExerciseAuthoringService service,
        CancellationToken cancellationToken) =>
        MapResult(
            await service.CreateVersionAsync(definitionId, request, cancellationToken),
            value => Results.Created(
                $"/api/v1/exercise-authoring/versions/{value.ExerciseVersionId}",
                value));

    private static async Task<IResult> PublishVersion(
        Guid versionId,
        ExerciseAuthoringService service,
        CancellationToken cancellationToken) =>
        MapResult(
            await service.PublishVersionAsync(versionId, cancellationToken),
            Results.Ok);

    private static IResult MapResult<T>(
        ExerciseResult<T> result,
        Func<T, IResult> success)
    {
        if (result.Kind == ExerciseResultKind.Success && result.Value is not null)
        {
            return success(result.Value);
        }

        var errors = result.Errors.ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal);
        return result.Kind switch
        {
            ExerciseResultKind.ValidationError => Results.ValidationProblem(errors),
            ExerciseResultKind.Conflict => Results.ValidationProblem(
                errors,
                statusCode: StatusCodes.Status409Conflict,
                title: "The exercise operation conflicts with the current state."),
            ExerciseResultKind.NotFound => Results.NotFound(),
            _ => Results.Problem(statusCode: StatusCodes.Status500InternalServerError),
        };
    }

    private static bool TryGetUserId(ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
