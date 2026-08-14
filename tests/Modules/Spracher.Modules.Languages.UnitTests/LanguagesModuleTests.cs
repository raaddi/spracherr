using Microsoft.Extensions.DependencyInjection;
using Spracher.Modules.Languages;

namespace Spracher.Modules.Languages.UnitTests;

public sealed class LanguagesModuleTests
{
    [Fact]
    public void AddLanguagesModuleShouldBeIdempotent()
    {
        var services = new ServiceCollection();

        services.AddLanguagesModule();
        services.AddLanguagesModule();

        Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(LanguagesModuleMarker));
    }
}
