using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Spracher.Contracts.Identity;
using Spracher.Modules.IdentityAccess.Application;
using Spracher.Modules.IdentityAccess.Email;

namespace Spracher.Modules.IdentityAccess;

public static class IdentityAccessEndpoints
{
    public static IEndpointRouteBuilder MapIdentityAccessEndpoints(
        this IEndpointRouteBuilder endpoints,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(environment);

        var auth = endpoints.MapGroup("/api/v1/auth").WithTags("Identity");

        auth.MapGet("/antiforgery", GetAntiforgeryToken)
            .WithName("GetAntiforgeryToken");
        auth.MapGet("/session", GetSession)
            .WithName("GetAuthSession");

        auth.MapPost("/register", Register)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .RequireRateLimiting(IdentityAccessModule.AuthRateLimitPolicy)
            .WithName("Register");
        auth.MapPost("/login", Login)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .RequireRateLimiting(IdentityAccessModule.AuthRateLimitPolicy)
            .WithName("Login");
        auth.MapPost("/logout", Logout)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .WithName("Logout");
        auth.MapPost("/confirm-email", ConfirmEmail)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .RequireRateLimiting(IdentityAccessModule.AuthRateLimitPolicy)
            .WithName("ConfirmEmail");
        auth.MapPost("/resend-confirmation", ResendConfirmation)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .RequireRateLimiting(IdentityAccessModule.AuthRateLimitPolicy)
            .WithName("ResendEmailConfirmation");
        auth.MapPost("/forgot-password", ForgotPassword)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .RequireRateLimiting(IdentityAccessModule.AuthRateLimitPolicy)
            .WithName("ForgotPassword");
        auth.MapPost("/reset-password", ResetPassword)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .RequireRateLimiting(IdentityAccessModule.AuthRateLimitPolicy)
            .WithName("ResetPassword");

        var profile = endpoints
            .MapGroup("/api/v1/profile")
            .RequireAuthorization()
            .WithTags("Profile");
        profile.MapGet("/", GetProfile).WithName("GetProfile");
        profile.MapPut("/", UpdateProfile)
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true))
            .WithName("UpdateProfile");

        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            auth.MapGet("/development-emails/latest", GetLatestDevelopmentEmail)
                .WithName("GetLatestDevelopmentEmail")
                .ExcludeFromDescription();
        }

        return endpoints;
    }

    private static Ok<AntiforgeryTokenResponse> GetAntiforgeryToken(
        HttpContext httpContext,
        IAntiforgery antiforgery)
    {
        httpContext.Response.Headers.CacheControl = "no-store";
        var tokens = antiforgery.GetAndStoreTokens(httpContext);
        return TypedResults.Ok(new AntiforgeryTokenResponse(
            tokens.RequestToken
            ?? throw new InvalidOperationException("Antiforgery request token was not generated.")));
    }

    private static async Task<Ok<AuthSessionResponse>> GetSession(
        HttpContext httpContext,
        IdentityAccountService accountService) =>
        TypedResults.Ok(await accountService.GetSessionAsync(httpContext.User));

    private static async Task<IResult> Register(
        RegisterRequest request,
        IdentityAccountService accountService,
        CancellationToken cancellationToken) =>
        MapResult(
            await accountService.RegisterAsync(request, cancellationToken),
            value => Results.Json(value, statusCode: StatusCodes.Status201Created));

    private static async Task<IResult> Login(
        LoginRequest request,
        IdentityAccountService accountService) =>
        MapResult(await accountService.LoginAsync(request), Results.Ok);

    private static async Task<Ok<OperationResponse>> Logout(
        IdentityAccountService accountService)
    {
        await accountService.LogoutAsync();
        return TypedResults.Ok(new OperationResponse("Signed out."));
    }

    private static async Task<IResult> GetProfile(
        HttpContext httpContext,
        IdentityAccountService accountService)
    {
        var session = await accountService.GetSessionAsync(httpContext.User);
        return session.User is null ? Results.Unauthorized() : Results.Ok(session.User);
    }

    private static async Task<IResult> UpdateProfile(
        HttpContext httpContext,
        UpdateProfileRequest request,
        IdentityAccountService accountService) =>
        MapResult(
            await accountService.UpdateProfileAsync(httpContext.User, request),
            value => Results.Ok(value.User));

    private static async Task<IResult> ConfirmEmail(
        ConfirmEmailRequest request,
        IdentityAccountService accountService) =>
        MapResult(await accountService.ConfirmEmailAsync(request), Results.Ok);

    private static async Task<IResult> ResendConfirmation(
        ResendConfirmationRequest request,
        IdentityAccountService accountService,
        CancellationToken cancellationToken) =>
        MapResult(
            await accountService.ResendConfirmationAsync(request, cancellationToken),
            Results.Ok);

    private static async Task<IResult> ForgotPassword(
        ForgotPasswordRequest request,
        IdentityAccountService accountService,
        CancellationToken cancellationToken) =>
        MapResult(
            await accountService.ForgotPasswordAsync(request, cancellationToken),
            Results.Ok);

    private static async Task<IResult> ResetPassword(
        ResetPasswordRequest request,
        IdentityAccountService accountService) =>
        MapResult(await accountService.ResetPasswordAsync(request), Results.Ok);

    private static IResult GetLatestDevelopmentEmail(
        string email,
        IDevelopmentEmailStore emailStore)
    {
        var message = string.IsNullOrWhiteSpace(email)
            ? null
            : emailStore.GetLatest(email);

        return message is null
            ? Results.NotFound()
            : Results.Ok(new DevelopmentEmailResponse(
                message.Recipient,
                message.Subject,
                message.ActionUrl,
                message.CreatedAt));
    }

    private static IResult MapResult<T>(
        AccountResult<T> result,
        Func<T, IResult> success)
    {
        if (result.Kind == AccountResultKind.Success && result.Value is not null)
        {
            return success(result.Value);
        }

        if (result.Kind == AccountResultKind.Accepted && result.Value is not null)
        {
            return Results.Accepted(value: result.Value);
        }

        var errors = result.Errors.ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.Ordinal);

        return result.Kind switch
        {
            AccountResultKind.ValidationError => Results.ValidationProblem(errors),
            AccountResultKind.Conflict => Results.ValidationProblem(
                errors,
                statusCode: StatusCodes.Status409Conflict,
                title: "The request conflicts with an existing account."),
            AccountResultKind.LockedOut => Results.Problem(
                statusCode: StatusCodes.Status429TooManyRequests,
                title: "The account is temporarily locked."),
            AccountResultKind.NotAllowed => Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Email confirmation is required."),
            _ => Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Invalid credentials."),
        };
    }
}
