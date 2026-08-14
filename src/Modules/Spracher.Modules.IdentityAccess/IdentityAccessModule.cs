using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Spracher.IdentityModel;
using Spracher.Modules.IdentityAccess.Application;
using Spracher.Modules.IdentityAccess.Email;

namespace Spracher.Modules.IdentityAccess;

public static class IdentityAccessModule
{
    public const string AdminPolicy = "AdminOnly";
    public const string TeacherPolicy = "TeacherOrAdmin";
    public const string AuthRateLimitPolicy = "IdentityAuth";

    public static IdentityBuilder AddIdentityAccessModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        services.TryAddSingleton<IdentityAccessModuleMarker>();

        ConfigureApplicationUrl(services, configuration);
        ConfigureEmailDelivery(services, configuration, environment);
        ConfigureAntiforgery(services, environment);

        services.AddScoped<IdentityAccountService>();
        services.AddScoped<IdentityLinkFactory>();

        services.AddAuthorizationBuilder()
            .AddPolicy(AdminPolicy, policy => policy.RequireRole(SystemRoles.Admin))
            .AddPolicy(
                TeacherPolicy,
                policy => policy.RequireRole(SystemRoles.Teacher, SystemRoles.Admin));

        var identityBuilder = services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;

                options.Password.RequiredLength = 10;
                options.Password.RequiredUniqueChars = 4;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddDefaultTokenProviders();

        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(24);
        });
        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.FromMinutes(5);
        });
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = environment.IsDevelopment()
                ? "Spracher.Auth"
                : "__Host-Spracher.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Path = "/";
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        return identityBuilder;
    }

    private static void ConfigureApplicationUrl(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<ApplicationUrlOptions>()
            .Bind(configuration.GetSection(ApplicationUrlOptions.SectionName))
            .Validate(
                options => Uri.TryCreate(
                    options.PublicUrl,
                    UriKind.Absolute,
                    out var uri)
                    && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
                "Application:PublicUrl must be an absolute HTTP or HTTPS URL.")
            .ValidateOnStart();
    }

    private static void ConfigureEmailDelivery(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var defaultMode = environment.IsDevelopment() || environment.IsEnvironment("Testing")
            ? "Development"
            : "Smtp";
        var mode = configuration["Email:Mode"] ?? defaultMode;

        if (string.Equals(mode, "Development", StringComparison.OrdinalIgnoreCase))
        {
            if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing"))
            {
                throw new InvalidOperationException(
                    "Development email delivery is allowed only in Development or Testing.");
            }

            services.AddSingleton<IDevelopmentEmailStore, DevelopmentEmailStore>();
            services.AddSingleton<IIdentityEmailSender, DevelopmentIdentityEmailSender>();
            return;
        }

        if (!string.Equals(mode, "Smtp", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Email:Mode must be 'Development' or 'Smtp'.");
        }

        services
            .AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Host),
                "Email:Smtp:Host is required.")
            .Validate(
                options => options.Port is > 0 and <= 65535,
                "Email:Smtp:Port must be a valid TCP port.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.FromAddress),
                "Email:Smtp:FromAddress is required.")
            .ValidateOnStart();
        services.AddScoped<IIdentityEmailSender, SmtpIdentityEmailSender>();
    }

    private static void ConfigureAntiforgery(
        IServiceCollection services,
        IHostEnvironment environment)
    {
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-XSRF-TOKEN";
            options.Cookie.Name = environment.IsDevelopment()
                ? "Spracher.Antiforgery"
                : "__Host-Spracher.Antiforgery";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Path = "/";
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
        });
    }
}

public sealed class IdentityAccessModuleMarker;
