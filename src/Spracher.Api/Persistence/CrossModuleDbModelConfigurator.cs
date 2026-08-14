using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Spracher.Modules.Languages.Domain;
using Spracher.Modules.Vocabulary.Domain;
using Spracher.Persistence;

namespace Spracher.Api.Persistence;

internal static class CrossModuleDbModelConfiguration
{
    public static IServiceCollection AddCrossModuleDbModelConfiguration(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IDbModelConfigurator, CrossModuleDbModelConfigurator>());
        return services;
    }
}

internal sealed class CrossModuleDbModelConfigurator : IDbModelConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<Lexeme>()
            .HasOne<Language>()
            .WithMany()
            .HasForeignKey(lexeme => lexeme.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LexemeSense>()
            .HasOne<Language>()
            .WithMany()
            .HasForeignKey(sense => sense.DefinitionLanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ExampleSentence>()
            .HasOne<Language>()
            .WithMany()
            .HasForeignKey(example => example.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
