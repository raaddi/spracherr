using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Spracher.Modules.Vocabulary.Application;
using Spracher.Modules.Vocabulary.Infrastructure;
using Spracher.Persistence;

namespace Spracher.Modules.Vocabulary;

public static class VocabularyModule
{
    public static IServiceCollection AddVocabularyModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<VocabularyModuleMarker>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IDbModelConfigurator, VocabularyDbModelConfigurator>());
        services.AddScoped<VocabularyService>();
        services.AddScoped<VocabularyCollectionService>();
        return services;
    }
}

public sealed class VocabularyModuleMarker;
