using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Spracher.BuildingBlocks.Languages;
using Spracher.Modules.Languages.Application;
using Spracher.Modules.Languages.Infrastructure;
using Spracher.Persistence;

namespace Spracher.Modules.Languages;

public static class LanguagesModule
{
    public static IServiceCollection AddLanguagesModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<LanguagesModuleMarker>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IDbModelConfigurator, LanguagesDbModelConfigurator>());
        services.AddScoped<LanguageProfileService>();
        services.AddScoped<ILanguageCatalogReader, LanguageCatalogReader>();
        return services;
    }
}

public sealed class LanguagesModuleMarker;
