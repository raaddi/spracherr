using System.Diagnostics;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Spracher.Api.Endpoints;
using Spracher.Api.ErrorHandling;
using Spracher.Api.Persistence;
using Spracher.BuildingBlocks.Time;
using Spracher.Modules.Exercises;
using Spracher.Modules.IdentityAccess;
using Spracher.Modules.Languages;
using Spracher.Modules.Vocabulary;
using Spracher.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffK";
    options.JsonWriterOptions = new JsonWriterOptions { Indented = false };
});

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

var dataProtectionBuilder = builder.Services
    .AddDataProtection()
    .SetApplicationName("Spracher");
var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
}

builder.Services
    .AddIdentityAccessModule(builder.Configuration, builder.Environment)
    .AddSpracherIdentityStores();
builder.Services.AddLanguagesModule();
builder.Services.AddVocabularyModule();
builder.Services.AddExercisesModule();
builder.Services.AddCrossModuleDbModelConfiguration();
builder.Services.AddSpracherPersistence(builder.Configuration);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(
        IdentityAccessModule.AuthRateLimitPolicy,
        httpContext => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1),
            }));
});

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<SpracherDbContext>(
        name: "postgresql",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"]);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

var app = builder.Build();

if (args.Contains("--migrate", StringComparer.OrdinalIgnoreCase))
{
    await app.Services.ApplySpracherMigrationsAsync();
    return;
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseRouting();
app.UseCors("WebClient");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseMiddleware<AntiforgeryValidationMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
});

app.MapSystemEndpoints();
app.MapIdentityAccessEndpoints(app.Environment);
app.MapLanguagesEndpoints();
app.MapVocabularyEndpoints();
app.MapExercisesEndpoints();

app.Run();

public partial class Program;
