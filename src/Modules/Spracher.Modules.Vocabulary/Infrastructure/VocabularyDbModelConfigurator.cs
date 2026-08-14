using Microsoft.EntityFrameworkCore;
using Spracher.IdentityModel;
using Spracher.Modules.Vocabulary.Domain;
using Spracher.Persistence;

namespace Spracher.Modules.Vocabulary.Infrastructure;

internal sealed class VocabularyDbModelConfigurator : IDbModelConfigurator
{
    private const string OwnershipConstraint =
        "(\"Visibility\" = 'Catalog' AND \"OwnerUserId\" IS NULL) OR "
        + "(\"Visibility\" = 'Private' AND \"OwnerUserId\" IS NOT NULL)";

    public void Configure(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        ConfigureConcept(modelBuilder);
        ConfigureLexeme(modelBuilder);
        ConfigureSense(modelBuilder);
        ConfigureLexemeParts(modelBuilder);
        ConfigureExamples(modelBuilder);
        ConfigureUserVocabulary(modelBuilder);
        ConfigureVocabularyOrganization(modelBuilder);
    }

    private static void ConfigureConcept(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Concept>(entity =>
        {
            entity.ToTable(
                "Concepts",
                "vocabulary",
                table => table.HasCheckConstraint("CK_Concepts_Ownership", OwnershipConstraint));
            entity.HasKey(concept => concept.Id);
            entity.Property(concept => concept.Key).HasMaxLength(200).IsRequired();
            entity.Property(concept => concept.Visibility).HasConversion<string>().HasMaxLength(20);
            entity.Property(concept => concept.SourceType).HasConversion<string>().HasMaxLength(20);
            entity.Property(concept => concept.SourceReference).HasMaxLength(200);
            entity.Property(concept => concept.PublicationStatus).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(concept => concept.Key).IsUnique();
            entity.HasIndex(concept => new { concept.OwnerUserId, concept.Visibility });
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(concept => concept.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasData(VocabularySeedData.Concepts);
        });
    }

    private static void ConfigureLexeme(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lexeme>(entity =>
        {
            entity.ToTable(
                "Lexemes",
                "vocabulary",
                table =>
                {
                    table.HasCheckConstraint("CK_Lexemes_Ownership", OwnershipConstraint);
                    table.HasCheckConstraint(
                        "CK_Lexemes_FrequencyRank",
                        "\"FrequencyRank\" IS NULL OR \"FrequencyRank\" > 0");
                });
            entity.HasKey(lexeme => lexeme.Id);
            entity.Property(lexeme => lexeme.Lemma).HasMaxLength(200).IsRequired();
            entity.Property(lexeme => lexeme.NormalizedLemma).HasMaxLength(200).IsRequired();
            entity.Property(lexeme => lexeme.PartOfSpeech).HasConversion<string>().HasMaxLength(30);
            entity.Property(lexeme => lexeme.CefrLevel).HasConversion<string>().HasMaxLength(2);
            entity.Property(lexeme => lexeme.Notes).HasMaxLength(2000);
            entity.Property(lexeme => lexeme.Visibility).HasConversion<string>().HasMaxLength(20);
            entity.Property(lexeme => lexeme.SourceType).HasConversion<string>().HasMaxLength(20);
            entity.Property(lexeme => lexeme.SourceReference).HasMaxLength(200);
            entity.Property(lexeme => lexeme.PublicationStatus).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(lexeme => new { lexeme.LanguageId, lexeme.NormalizedLemma })
                .HasOperators("uuid_ops", "text_pattern_ops");
            entity.HasIndex(lexeme => new
            {
                lexeme.LanguageId,
                lexeme.PartOfSpeech,
                lexeme.NormalizedLemma,
            });
            entity.HasIndex(lexeme => new { lexeme.PublicationStatus, lexeme.CefrLevel });
            entity.HasIndex(lexeme => lexeme.FrequencyRank);
            entity.HasIndex(lexeme => new
            {
                lexeme.OwnerUserId,
                lexeme.LanguageId,
                lexeme.PartOfSpeech,
                lexeme.NormalizedLemma,
            })
                .IsUnique()
                .HasFilter("\"OwnerUserId\" IS NOT NULL");
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(lexeme => lexeme.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasData(VocabularySeedData.Lexemes);
        });
    }

    private static void ConfigureSense(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LexemeSense>(entity =>
        {
            entity.ToTable(
                "LexemeSenses",
                "vocabulary",
                table => table.HasCheckConstraint(
                    "CK_LexemeSenses_Ownership",
                    OwnershipConstraint));
            entity.HasKey(sense => sense.Id);
            entity.Property(sense => sense.Definition).HasMaxLength(1000).IsRequired();
            entity.Property(sense => sense.Register).HasMaxLength(100);
            entity.Property(sense => sense.CefrLevelOverride).HasConversion<string>().HasMaxLength(2);
            entity.Property(sense => sense.Visibility).HasConversion<string>().HasMaxLength(20);
            entity.Property(sense => sense.PublicationStatus).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(sense => new { sense.LexemeId, sense.ConceptId });
            entity.HasIndex(sense => sense.ConceptId);
            entity.HasOne<Lexeme>()
                .WithMany()
                .HasForeignKey(sense => sense.LexemeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Concept>()
                .WithMany()
                .HasForeignKey(sense => sense.ConceptId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(sense => sense.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasData(VocabularySeedData.Senses);
        });
    }

    private static void ConfigureLexemeParts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WordForm>(entity =>
        {
            entity.ToTable("WordForms", "vocabulary");
            entity.HasKey(form => form.Id);
            entity.Property(form => form.Form).HasMaxLength(200).IsRequired();
            entity.Property(form => form.NormalizedForm).HasMaxLength(200).IsRequired();
            entity.Property(form => form.GrammarTags).HasMaxLength(300).IsRequired();
            entity.HasIndex(form => new { form.LexemeId, form.NormalizedForm });
            entity.HasOne<Lexeme>()
                .WithMany()
                .HasForeignKey(form => form.LexemeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasData(VocabularySeedData.WordForms);
        });

        modelBuilder.Entity<Pronunciation>(entity =>
        {
            entity.ToTable("Pronunciations", "vocabulary");
            entity.HasKey(pronunciation => pronunciation.Id);
            entity.Property(pronunciation => pronunciation.Scheme).HasMaxLength(30).IsRequired();
            entity.Property(pronunciation => pronunciation.Value).HasMaxLength(300).IsRequired();
            entity.Property(pronunciation => pronunciation.Region).HasMaxLength(35);
            entity.Property(pronunciation => pronunciation.AudioAssetReference).HasMaxLength(500);
            entity.HasOne<Lexeme>()
                .WithMany()
                .HasForeignKey(pronunciation => pronunciation.LexemeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasData(VocabularySeedData.Pronunciations);
        });

        modelBuilder.Entity<LexemeFeature>(entity =>
        {
            entity.ToTable("LexemeFeatures", "vocabulary");
            entity.HasKey(feature => feature.Id);
            entity.Property(feature => feature.Key).HasMaxLength(80).IsRequired();
            entity.Property(feature => feature.Value).HasMaxLength(300).IsRequired();
            entity.HasIndex(feature => new { feature.LexemeId, feature.Key, feature.Value })
                .IsUnique();
            entity.HasOne<Lexeme>()
                .WithMany()
                .HasForeignKey(feature => feature.LexemeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasData(VocabularySeedData.Features);
        });
    }

    private static void ConfigureExamples(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExampleSentence>(entity =>
        {
            entity.ToTable(
                "ExampleSentences",
                "vocabulary",
                table => table.HasCheckConstraint(
                    "CK_ExampleSentences_Ownership",
                    OwnershipConstraint));
            entity.HasKey(example => example.Id);
            entity.Property(example => example.Text).HasMaxLength(2000).IsRequired();
            entity.Property(example => example.SourceReference).HasMaxLength(200);
            entity.Property(example => example.Visibility).HasConversion<string>().HasMaxLength(20);
            entity.Property(example => example.PublicationStatus).HasConversion<string>().HasMaxLength(20);
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(example => example.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasData(VocabularySeedData.Examples);
        });

        modelBuilder.Entity<ExampleUsage>(entity =>
        {
            entity.ToTable(
                "ExampleUsages",
                "vocabulary",
                table => table.HasCheckConstraint(
                    "CK_ExampleUsages_Highlight",
                    "(\"HighlightStart\" IS NULL AND \"HighlightLength\" IS NULL) OR "
                    + "(\"HighlightStart\" >= 0 AND \"HighlightLength\" > 0)"));
            entity.HasKey(usage => new { usage.LexemeSenseId, usage.ExampleSentenceId });
            entity.HasOne<LexemeSense>()
                .WithMany()
                .HasForeignKey(usage => usage.LexemeSenseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ExampleSentence>()
                .WithMany()
                .HasForeignKey(usage => usage.ExampleSentenceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasData(VocabularySeedData.ExampleUsages);
        });
    }

    private static void ConfigureUserVocabulary(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserVocabularyItem>(entity =>
        {
            entity.ToTable("UserVocabularyItems", "vocabulary");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(item => new { item.UserId, item.LexemeSenseId }).IsUnique();
            entity.HasIndex(item => new { item.UserId, item.Status, item.StatusChangedAt });
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<LexemeSense>()
                .WithMany()
                .HasForeignKey(item => item.LexemeSenseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureVocabularyOrganization(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VocabularyList>(entity =>
        {
            entity.ToTable("VocabularyLists", "vocabulary");
            entity.HasKey(list => list.Id);
            entity.Property(list => list.Name).HasMaxLength(100).IsRequired();
            entity.Property(list => list.NormalizedName).HasMaxLength(100).IsRequired();
            entity.Property(list => list.Description).HasMaxLength(500);
            entity.HasIndex(list => new { list.OwnerUserId, list.NormalizedName }).IsUnique();
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(list => list.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VocabularyListItem>(entity =>
        {
            entity.ToTable(
                "VocabularyListItems",
                "vocabulary",
                table => table.HasCheckConstraint(
                    "CK_VocabularyListItems_Position",
                    "\"Position\" >= 0"));
            entity.HasKey(item => new { item.VocabularyListId, item.LexemeSenseId });
            entity.Property(item => item.Note).HasMaxLength(500);
            entity.HasIndex(item => new { item.VocabularyListId, item.Position });
            entity.HasOne<VocabularyList>()
                .WithMany()
                .HasForeignKey(item => item.VocabularyListId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<LexemeSense>()
                .WithMany()
                .HasForeignKey(item => item.LexemeSenseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VocabularyCategory>(entity =>
        {
            entity.ToTable("VocabularyCategories", "vocabulary");
            entity.HasKey(category => category.Id);
            entity.Property(category => category.Name).HasMaxLength(60).IsRequired();
            entity.Property(category => category.NormalizedName).HasMaxLength(60).IsRequired();
            entity.Property(category => category.Color).HasMaxLength(7).IsFixedLength().IsRequired();
            entity.HasIndex(category => new
            {
                category.OwnerUserId,
                category.NormalizedName,
            }).IsUnique();
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(category => category.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserVocabularyItemCategory>(entity =>
        {
            entity.ToTable("UserVocabularyItemCategories", "vocabulary");
            entity.HasKey(assignment => new
            {
                assignment.UserVocabularyItemId,
                assignment.VocabularyCategoryId,
            });
            entity.HasIndex(assignment => assignment.VocabularyCategoryId);
            entity.HasOne<UserVocabularyItem>()
                .WithMany()
                .HasForeignKey(assignment => assignment.UserVocabularyItemId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<VocabularyCategory>()
                .WithMany()
                .HasForeignKey(assignment => assignment.VocabularyCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
