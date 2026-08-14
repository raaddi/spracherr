using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Spracher.Modules.Exercises.Application;
using Spracher.Modules.Exercises.Engine;
using Spracher.Modules.Exercises.Infrastructure;
using Spracher.Persistence;

namespace Spracher.Modules.Exercises;

public static class ExercisesModule
{
    public static IServiceCollection AddExercisesModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<ExercisesModuleMarker>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IDbModelConfigurator, ExercisesDbModelConfigurator>());
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IExerciseTypeHandler, MultipleChoiceExerciseHandler>());
        services.AddSingleton<ExerciseTypeRegistry>();
        services.AddScoped<ExerciseService>();
        return services;
    }
}

public sealed class ExercisesModuleMarker;
