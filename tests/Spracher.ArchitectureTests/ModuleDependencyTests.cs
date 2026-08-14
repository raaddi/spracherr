using System.Reflection;
using Spracher.IdentityModel;
using Spracher.Modules.Exercises;
using Spracher.Modules.IdentityAccess;
using Spracher.Modules.Languages;
using Spracher.Modules.Vocabulary;
using Spracher.Persistence;

namespace Spracher.ArchitectureTests;

public sealed class ModuleDependencyTests
{
    private static readonly Assembly[] ModuleAssemblies =
    [
        typeof(IdentityAccessModule).Assembly,
        typeof(ExercisesModule).Assembly,
        typeof(LanguagesModule).Assembly,
        typeof(VocabularyModule).Assembly,
    ];

    [Fact]
    public void BusinessModulesShouldNotReferenceOtherBusinessModules()
    {
        foreach (var moduleAssembly in ModuleAssemblies)
        {
            var illegalReferences = moduleAssembly
                .GetReferencedAssemblies()
                .Where(reference =>
                    reference.Name?.StartsWith(
                        "Spracher.Modules.",
                        StringComparison.Ordinal) == true
                    && !string.Equals(
                        reference.Name,
                        moduleAssembly.GetName().Name,
                        StringComparison.Ordinal))
                .Select(reference => reference.Name)
                .ToArray();

            Assert.Empty(illegalReferences);
        }
    }

    [Fact]
    public void PersistenceShouldNotReferenceBusinessModules()
    {
        var illegalReferences = typeof(SpracherDbContext)
            .Assembly
            .GetReferencedAssemblies()
            .Where(reference =>
                reference.Name?.StartsWith(
                    "Spracher.Modules.",
                    StringComparison.Ordinal) == true)
            .Select(reference => reference.Name)
            .ToArray();

        Assert.Empty(illegalReferences);
    }

    [Fact]
    public void IdentityStorageModelShouldNotReferenceModulesOrPersistence()
    {
        var illegalReferences = typeof(ApplicationUser)
            .Assembly
            .GetReferencedAssemblies()
            .Where(reference =>
                reference.Name?.StartsWith(
                    "Spracher.Modules.",
                    StringComparison.Ordinal) == true
                || string.Equals(
                    reference.Name,
                    typeof(SpracherDbContext).Assembly.GetName().Name,
                    StringComparison.Ordinal))
            .Select(reference => reference.Name)
            .ToArray();

        Assert.Empty(illegalReferences);
    }
}
