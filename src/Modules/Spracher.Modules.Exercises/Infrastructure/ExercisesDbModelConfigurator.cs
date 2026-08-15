using Microsoft.EntityFrameworkCore;
using Spracher.IdentityModel;
using Spracher.Modules.Exercises.Domain;
using Spracher.Persistence;

namespace Spracher.Modules.Exercises.Infrastructure;

internal sealed class ExercisesDbModelConfigurator : IDbModelConfigurator
{
    public void Configure(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<ExerciseDefinition>(entity =>
        {
            entity.ToTable("ExerciseDefinitions", "exercises");
            entity.HasKey(definition => definition.Id);
            entity.Property(definition => definition.TypeKey).HasMaxLength(80).IsRequired();
            entity.Property(definition => definition.Title).HasMaxLength(200).IsRequired();
            entity.Property(definition => definition.Description).HasMaxLength(1000);
            entity.HasIndex(definition => new { definition.TypeKey, definition.ArchivedAt });
            entity.HasIndex(definition => definition.OwnerUserId);
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(definition => definition.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasData(ExerciseSeedData.Definitions);
        });

        modelBuilder.Entity<ExerciseVersion>(entity =>
        {
            entity.ToTable(
                "ExerciseVersions",
                "exercises",
                table =>
                {
                    table.HasCheckConstraint("CK_ExerciseVersions_Version", "\"VersionNumber\" > 0");
                    table.HasCheckConstraint("CK_ExerciseVersions_Schema", "\"SchemaVersion\" > 0");
                    table.HasCheckConstraint(
                        "CK_ExerciseVersions_Publication",
                        "(\"Status\" = 'Draft' AND \"PublishedAt\" IS NULL) OR "
                        + "(\"Status\" IN ('Published', 'Archived') AND \"PublishedAt\" IS NOT NULL)");
                });
            entity.HasKey(version => version.Id);
            entity.Property(version => version.Prompt).HasMaxLength(2000).IsRequired();
            entity.Property(version => version.DefinitionJson).HasColumnType("jsonb").IsRequired();
            entity.Property(version => version.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(version => new
            {
                version.ExerciseDefinitionId,
                version.VersionNumber,
            }).IsUnique();
            entity.HasIndex(version => new
            {
                version.ExerciseDefinitionId,
                version.Status,
                version.VersionNumber,
            });
            entity.HasIndex(version => version.ExerciseDefinitionId)
                .IsUnique()
                .HasFilter("\"Status\" = 'Draft'")
                .HasDatabaseName("UX_ExerciseVersions_OneDraftPerDefinition");
            entity.HasOne<ExerciseDefinition>()
                .WithMany()
                .HasForeignKey(version => version.ExerciseDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasData(ExerciseSeedData.Versions);
        });

        modelBuilder.Entity<ExerciseSet>(entity =>
        {
            entity.ToTable(
                "ExerciseSets",
                "exercises",
                table => table.HasCheckConstraint(
                    "CK_ExerciseSets_Publication",
                    "(\"Status\" = 'Draft' AND \"PublishedAt\" IS NULL) OR "
                    + "(\"Status\" IN ('Published', 'Archived') "
                    + "AND \"PublishedAt\" IS NOT NULL)"));
            entity.HasKey(set => set.Id);
            entity.Property(set => set.Title).HasMaxLength(200).IsRequired();
            entity.Property(set => set.Description).HasMaxLength(1000);
            entity.Property(set => set.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(set => new { set.Status, set.PublishedAt });
            entity.HasIndex(set => set.OwnerUserId);
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(set => set.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasData(ExerciseSeedData.Sets);
        });

        modelBuilder.Entity<ExerciseSetItem>(entity =>
        {
            entity.ToTable(
                "ExerciseSetItems",
                "exercises",
                table => table.HasCheckConstraint(
                    "CK_ExerciseSetItems_Position",
                    "\"Position\" > 0"));
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => new { item.ExerciseSetId, item.Position }).IsUnique();
            entity.HasIndex(item => new { item.ExerciseSetId, item.ExerciseVersionId })
                .IsUnique();
            entity.HasIndex(item => item.ExerciseVersionId);
            entity.HasOne<ExerciseSet>()
                .WithMany()
                .HasForeignKey(item => item.ExerciseSetId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ExerciseVersion>()
                .WithMany()
                .HasForeignKey(item => item.ExerciseVersionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasData(ExerciseSeedData.SetItems);
        });

        modelBuilder.Entity<ExerciseAttempt>(entity =>
        {
            entity.ToTable(
                "ExerciseAttempts",
                "exercises",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_ExerciseAttempts_Score",
                        "(\"AwardedPoints\" IS NULL AND \"MaxPoints\" IS NULL) OR "
                        + "(\"AwardedPoints\" >= 0 AND \"MaxPoints\" > 0 "
                        + "AND \"AwardedPoints\" <= \"MaxPoints\")");
                    table.HasCheckConstraint(
                        "CK_ExerciseAttempts_Lifecycle",
                        "(\"Status\" = 'InProgress' AND \"CompletedAt\" IS NULL "
                        + "AND \"AwardedPoints\" IS NULL AND \"MaxPoints\" IS NULL) OR "
                        + "(\"Status\" = 'Completed' AND \"CompletedAt\" IS NOT NULL "
                        + "AND \"AwardedPoints\" IS NOT NULL AND \"MaxPoints\" IS NOT NULL)");
                });
            entity.HasKey(attempt => attempt.Id);
            entity.Property(attempt => attempt.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(attempt => new { attempt.UserId, attempt.StartedAt });
            entity.HasIndex(attempt => attempt.ExerciseVersionId);
            entity.HasIndex(attempt => attempt.ExerciseSetItemId);
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(attempt => attempt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ExerciseVersion>()
                .WithMany()
                .HasForeignKey(attempt => attempt.ExerciseVersionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ExerciseSetItem>()
                .WithMany()
                .HasForeignKey(attempt => attempt.ExerciseSetItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ExerciseSubmission>(entity =>
        {
            entity.ToTable(
                "ExerciseSubmissions",
                "exercises",
                table => table.HasCheckConstraint(
                    "CK_ExerciseSubmissions_Score",
                    "\"AwardedPoints\" >= 0 AND \"MaxPoints\" > 0 "
                    + "AND \"AwardedPoints\" <= \"MaxPoints\""));
            entity.HasKey(submission => submission.Id);
            entity.Property(submission => submission.ResponseJson).HasColumnType("jsonb").IsRequired();
            entity.Property(submission => submission.GradingJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(submission => submission.AttemptId).IsUnique();
            entity.HasOne<ExerciseAttempt>()
                .WithOne()
                .HasForeignKey<ExerciseSubmission>(submission => submission.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
