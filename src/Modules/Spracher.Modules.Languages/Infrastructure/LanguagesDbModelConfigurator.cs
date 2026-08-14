using Microsoft.EntityFrameworkCore;
using Spracher.IdentityModel;
using Spracher.Modules.Languages.Domain;
using Spracher.Persistence;

namespace Spracher.Modules.Languages.Infrastructure;

internal sealed class LanguagesDbModelConfigurator : IDbModelConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<Language>(entity =>
        {
            entity.ToTable(
                "Languages",
                "languages",
                table => table.HasCheckConstraint(
                    "CK_Languages_TextDirection",
                    "\"TextDirection\" IN ('LeftToRight', 'RightToLeft')"));
            entity.HasKey(language => language.Id);
            entity.Property(language => language.Code).HasMaxLength(35).IsRequired();
            entity.Property(language => language.Name).HasMaxLength(100).IsRequired();
            entity.Property(language => language.NativeName).HasMaxLength(100).IsRequired();
            entity.Property(language => language.TextDirection)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            entity.HasIndex(language => language.Code).IsUnique();
            entity.HasData(LanguageSeedData.All);
        });

        modelBuilder.Entity<UserLanguageProfile>(entity =>
        {
            entity.ToTable(
                "UserLanguageProfiles",
                "languages",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_UserLanguageProfiles_Selected",
                        "\"IsNative\" OR \"IsLearning\"");
                    table.HasCheckConstraint(
                        "CK_UserLanguageProfiles_LearningState",
                        "(\"IsLearning\" AND \"CurrentCefrLevel\" IS NOT NULL AND \"StartedAt\" IS NOT NULL) OR "
                        + "(NOT \"IsLearning\" AND \"CurrentCefrLevel\" IS NULL AND \"StartedAt\" IS NULL)");
                    table.HasCheckConstraint(
                        "CK_UserLanguageProfiles_CefrLevel",
                        "\"CurrentCefrLevel\" IS NULL OR \"CurrentCefrLevel\" IN ('A0', 'A1', 'A2', 'B1', 'B2', 'C1', 'C2')");
                });
            entity.HasKey(profile => profile.Id);
            entity.Property(profile => profile.CurrentCefrLevel)
                .HasConversion<string>()
                .HasMaxLength(2);
            entity.HasIndex(profile => new { profile.UserId, profile.LanguageId })
                .IsUnique();
            entity.HasOne<Language>()
                .WithMany()
                .HasForeignKey(profile => profile.LanguageId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(profile => profile.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
