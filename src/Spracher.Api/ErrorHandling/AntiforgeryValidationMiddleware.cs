using Microsoft.AspNetCore.Antiforgery;

namespace Spracher.Api.ErrorHandling;

public sealed class AntiforgeryValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var metadata = httpContext.GetEndpoint()?
            .Metadata.GetMetadata<IAntiforgeryMetadata>();
        if (metadata?.RequiresValidation == true)
        {
            var validation = httpContext.Features.Get<IAntiforgeryValidationFeature>();
            if (validation?.IsValid != true)
            {
                await TypedResults.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "The antiforgery token is invalid or missing.")
                    .ExecuteAsync(httpContext);
                return;
            }
        }

        await next(httpContext);
    }
}
