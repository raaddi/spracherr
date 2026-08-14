using Microsoft.Extensions.DependencyInjection;
using Spracher.Modules.Exercises.Engine;

namespace Spracher.Modules.Exercises.UnitTests;

public sealed class ExercisesModuleTests
{
    [Fact]
    public void AddExercisesModuleShouldBeIdempotent()
    {
        var services = new ServiceCollection();

        services.AddExercisesModule();
        services.AddExercisesModule();

        Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(ExercisesModuleMarker));
        Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(IExerciseTypeHandler));
    }
}
