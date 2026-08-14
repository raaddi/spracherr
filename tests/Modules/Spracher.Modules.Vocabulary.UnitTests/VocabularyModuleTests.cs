using Microsoft.Extensions.DependencyInjection;
using Spracher.Modules.Vocabulary;

namespace Spracher.Modules.Vocabulary.UnitTests;

public sealed class VocabularyModuleTests
{
    [Fact]
    public void AddVocabularyModuleShouldBeIdempotent()
    {
        var services = new ServiceCollection();

        services.AddVocabularyModule();
        services.AddVocabularyModule();

        Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(VocabularyModuleMarker));
    }
}
