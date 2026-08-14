using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA1861 // EF migration scaffolding uses repeated array literals
#pragma warning disable IDE0161 // EF migration scaffolding uses block-scoped namespaces

namespace Spracher.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVocabularyFirstSlice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "vocabulary");

            migrationBuilder.CreateTable(
                name: "Concepts",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SourceReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PublicationStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Concepts", x => x.Id);
                    table.CheckConstraint("CK_Concepts_Ownership", "(\"Visibility\" = 'Catalog' AND \"OwnerUserId\" IS NULL) OR (\"Visibility\" = 'Private' AND \"OwnerUserId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Concepts_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalSchema: "iam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExampleSentences",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SourceReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PublicationStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExampleSentences", x => x.Id);
                    table.CheckConstraint("CK_ExampleSentences_Ownership", "(\"Visibility\" = 'Catalog' AND \"OwnerUserId\" IS NULL) OR (\"Visibility\" = 'Private' AND \"OwnerUserId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ExampleSentences_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalSchema: "languages",
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExampleSentences_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalSchema: "iam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lexemes",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Lemma = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NormalizedLemma = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PartOfSpeech = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CefrLevel = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    FrequencyRank = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SourceReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PublicationStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lexemes", x => x.Id);
                    table.CheckConstraint("CK_Lexemes_FrequencyRank", "\"FrequencyRank\" IS NULL OR \"FrequencyRank\" > 0");
                    table.CheckConstraint("CK_Lexemes_Ownership", "(\"Visibility\" = 'Catalog' AND \"OwnerUserId\" IS NULL) OR (\"Visibility\" = 'Private' AND \"OwnerUserId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Lexemes_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalSchema: "languages",
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lexemes_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalSchema: "iam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LexemeFeatures",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LexemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Value = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LexemeFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LexemeFeatures_Lexemes_LexemeId",
                        column: x => x.LexemeId,
                        principalSchema: "vocabulary",
                        principalTable: "Lexemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LexemeSenses",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LexemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConceptId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionLanguageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Definition = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Register = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CefrLevelOverride = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PublicationStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LexemeSenses", x => x.Id);
                    table.CheckConstraint("CK_LexemeSenses_Ownership", "(\"Visibility\" = 'Catalog' AND \"OwnerUserId\" IS NULL) OR (\"Visibility\" = 'Private' AND \"OwnerUserId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_LexemeSenses_Concepts_ConceptId",
                        column: x => x.ConceptId,
                        principalSchema: "vocabulary",
                        principalTable: "Concepts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LexemeSenses_Languages_DefinitionLanguageId",
                        column: x => x.DefinitionLanguageId,
                        principalSchema: "languages",
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LexemeSenses_Lexemes_LexemeId",
                        column: x => x.LexemeId,
                        principalSchema: "vocabulary",
                        principalTable: "Lexemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LexemeSenses_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalSchema: "iam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pronunciations",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LexemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Scheme = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Value = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Region = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    AudioAssetReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pronunciations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pronunciations_Lexemes_LexemeId",
                        column: x => x.LexemeId,
                        principalSchema: "vocabulary",
                        principalTable: "Lexemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WordForms",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LexemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Form = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NormalizedForm = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    GrammarTags = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WordForms_Lexemes_LexemeId",
                        column: x => x.LexemeId,
                        principalSchema: "vocabulary",
                        principalTable: "Lexemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExampleUsages",
                schema: "vocabulary",
                columns: table => new
                {
                    LexemeSenseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExampleSentenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    HighlightStart = table.Column<int>(type: "integer", nullable: true),
                    HighlightLength = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExampleUsages", x => new { x.LexemeSenseId, x.ExampleSentenceId });
                    table.CheckConstraint("CK_ExampleUsages_Highlight", "(\"HighlightStart\" IS NULL AND \"HighlightLength\" IS NULL) OR (\"HighlightStart\" >= 0 AND \"HighlightLength\" > 0)");
                    table.ForeignKey(
                        name: "FK_ExampleUsages_ExampleSentences_ExampleSentenceId",
                        column: x => x.ExampleSentenceId,
                        principalSchema: "vocabulary",
                        principalTable: "ExampleSentences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExampleUsages_LexemeSenses_LexemeSenseId",
                        column: x => x.LexemeSenseId,
                        principalSchema: "vocabulary",
                        principalTable: "LexemeSenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserVocabularyItems",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LexemeSenseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AddedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StatusChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVocabularyItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserVocabularyItems_LexemeSenses_LexemeSenseId",
                        column: x => x.LexemeSenseId,
                        principalSchema: "vocabulary",
                        principalTable: "LexemeSenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserVocabularyItems_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "iam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "vocabulary",
                table: "Concepts",
                columns: new[] { "Id", "CreatedAt", "Key", "OwnerUserId", "PublicationStatus", "SourceReference", "SourceType", "Visibility" },
                values: new object[,]
                {
                    { new Guid("0198ae00-0000-7000-8000-000000000001"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bank.financial-institution", null, "Published", "spracher-curated-en-pl-v1", "Curated", "Catalog" },
                    { new Guid("0198ae00-0000-7000-8000-000000000002"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bank.river-edge", null, "Published", "spracher-curated-en-pl-v1", "Curated", "Catalog" },
                    { new Guid("0198ae00-0000-7000-8000-000000000003"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "run.move-quickly", null, "Published", "spracher-curated-en-pl-v1", "Curated", "Catalog" },
                    { new Guid("0198ae00-0000-7000-8000-000000000004"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "apple.fruit", null, "Published", "spracher-curated-en-pl-v1", "Curated", "Catalog" }
                });

            migrationBuilder.InsertData(
                schema: "vocabulary",
                table: "ExampleSentences",
                columns: new[] { "Id", "CreatedAt", "LanguageId", "OwnerUserId", "PublicationStatus", "SourceReference", "Text", "Visibility" },
                values: new object[,]
                {
                    { new Guid("0198ae60-0000-7000-8000-000000000001"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0198ac50-0000-7000-8000-000000000002"), null, "Published", "spracher-curated-en-pl-v1", "She works at a bank in the city centre.", "Catalog" },
                    { new Guid("0198ae60-0000-7000-8000-000000000002"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0198ac50-0000-7000-8000-000000000002"), null, "Published", "spracher-curated-en-pl-v1", "We sat on the river bank.", "Catalog" },
                    { new Guid("0198ae60-0000-7000-8000-000000000003"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0198ac50-0000-7000-8000-000000000001"), null, "Published", "spracher-curated-en-pl-v1", "Usiedliśmy na brzegu rzeki.", "Catalog" },
                    { new Guid("0198ae60-0000-7000-8000-000000000004"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0198ac50-0000-7000-8000-000000000002"), null, "Published", "spracher-curated-en-pl-v1", "I run every morning.", "Catalog" },
                    { new Guid("0198ae60-0000-7000-8000-000000000005"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0198ac50-0000-7000-8000-000000000001"), null, "Published", "spracher-curated-en-pl-v1", "Lubię biec rano.", "Catalog" },
                    { new Guid("0198ae60-0000-7000-8000-000000000006"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("0198ac50-0000-7000-8000-000000000002"), null, "Published", "spracher-curated-en-pl-v1", "This apple is sweet.", "Catalog" }
                });

            migrationBuilder.InsertData(
                schema: "vocabulary",
                table: "Lexemes",
                columns: new[] { "Id", "CefrLevel", "CreatedAt", "FrequencyRank", "LanguageId", "Lemma", "NormalizedLemma", "Notes", "OwnerUserId", "PartOfSpeech", "PublicationStatus", "SourceReference", "SourceType", "Visibility" },
                values: new object[,]
                {
                    { new Guid("0198ae10-0000-7000-8000-000000000001"), "A2", new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 520, new Guid("0198ac50-0000-7000-8000-000000000002"), "bank", "bank", null, null, "Noun", "Published", "spracher-curated-en-pl-v1", "Curated", "Catalog" },
                    { new Guid("0198ae10-0000-7000-8000-000000000002"), "A2", new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 780, new Guid("0198ac50-0000-7000-8000-000000000001"), "bank", "bank", null, null, "Noun", "Published", "spracher-curated-en-pl-v1", "Curated", "Catalog" },
                    { new Guid("0198ae10-0000-7000-8000-000000000003"), "A2", new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 690, new Guid("0198ac50-0000-7000-8000-000000000001"), "brzeg", "brzeg", null, null, "Noun", "Published", "spracher-curated-en-pl-v1", "Curated", "Catalog" },
                    { new Guid("0198ae10-0000-7000-8000-000000000004"), "A1", new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 180, new Guid("0198ac50-0000-7000-8000-000000000002"), "run", "run", null, null, "Verb", "Published", "spracher-curated-en-pl-v1", "Curated", "Catalog" },
                    { new Guid("0198ae10-0000-7000-8000-000000000005"), "A1", new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 410, new Guid("0198ac50-0000-7000-8000-000000000001"), "biec", "biec", null, null, "Verb", "Published", "spracher-curated-en-pl-v1", "Curated", "Catalog" },
                    { new Guid("0198ae10-0000-7000-8000-000000000006"), "A1", new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1120, new Guid("0198ac50-0000-7000-8000-000000000002"), "apple", "apple", null, null, "Noun", "Published", "spracher-curated-en-pl-v1", "Curated", "Catalog" },
                    { new Guid("0198ae10-0000-7000-8000-000000000007"), "A1", new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 990, new Guid("0198ac50-0000-7000-8000-000000000001"), "jabłko", "jabłko", null, null, "Noun", "Published", "spracher-curated-en-pl-v1", "Curated", "Catalog" }
                });

            migrationBuilder.InsertData(
                schema: "vocabulary",
                table: "LexemeFeatures",
                columns: new[] { "Id", "Key", "LexemeId", "Value" },
                values: new object[,]
                {
                    { new Guid("0198ae50-0000-7000-8000-000000000001"), "gender", new Guid("0198ae10-0000-7000-8000-000000000002"), "masculine" },
                    { new Guid("0198ae50-0000-7000-8000-000000000002"), "gender", new Guid("0198ae10-0000-7000-8000-000000000003"), "masculine" },
                    { new Guid("0198ae50-0000-7000-8000-000000000003"), "gender", new Guid("0198ae10-0000-7000-8000-000000000007"), "neuter" }
                });

            migrationBuilder.InsertData(
                schema: "vocabulary",
                table: "LexemeSenses",
                columns: new[] { "Id", "CefrLevelOverride", "ConceptId", "CreatedAt", "Definition", "DefinitionLanguageId", "LexemeId", "OwnerUserId", "PublicationStatus", "Register", "Visibility" },
                values: new object[,]
                {
                    { new Guid("0198ae20-0000-7000-8000-000000000001"), null, new Guid("0198ae00-0000-7000-8000-000000000001"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "An organization that keeps, lends, and exchanges money.", new Guid("0198ac50-0000-7000-8000-000000000002"), new Guid("0198ae10-0000-7000-8000-000000000001"), null, "Published", null, "Catalog" },
                    { new Guid("0198ae20-0000-7000-8000-000000000002"), null, new Guid("0198ae00-0000-7000-8000-000000000002"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "The land along the edge of a river.", new Guid("0198ac50-0000-7000-8000-000000000002"), new Guid("0198ae10-0000-7000-8000-000000000001"), null, "Published", null, "Catalog" },
                    { new Guid("0198ae20-0000-7000-8000-000000000003"), null, new Guid("0198ae00-0000-7000-8000-000000000001"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Instytucja przechowująca pieniądze i udzielająca pożyczek.", new Guid("0198ac50-0000-7000-8000-000000000001"), new Guid("0198ae10-0000-7000-8000-000000000002"), null, "Published", null, "Catalog" },
                    { new Guid("0198ae20-0000-7000-8000-000000000004"), null, new Guid("0198ae00-0000-7000-8000-000000000002"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Pas lądu znajdujący się przy rzece.", new Guid("0198ac50-0000-7000-8000-000000000001"), new Guid("0198ae10-0000-7000-8000-000000000003"), null, "Published", null, "Catalog" },
                    { new Guid("0198ae20-0000-7000-8000-000000000005"), null, new Guid("0198ae00-0000-7000-8000-000000000003"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "To move quickly on foot.", new Guid("0198ac50-0000-7000-8000-000000000002"), new Guid("0198ae10-0000-7000-8000-000000000004"), null, "Published", null, "Catalog" },
                    { new Guid("0198ae20-0000-7000-8000-000000000006"), null, new Guid("0198ae00-0000-7000-8000-000000000003"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Poruszać się szybko, odbijając się stopami od podłoża.", new Guid("0198ac50-0000-7000-8000-000000000001"), new Guid("0198ae10-0000-7000-8000-000000000005"), null, "Published", null, "Catalog" },
                    { new Guid("0198ae20-0000-7000-8000-000000000007"), null, new Guid("0198ae00-0000-7000-8000-000000000004"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "A round fruit with firm flesh and thin skin.", new Guid("0198ac50-0000-7000-8000-000000000002"), new Guid("0198ae10-0000-7000-8000-000000000006"), null, "Published", null, "Catalog" },
                    { new Guid("0198ae20-0000-7000-8000-000000000008"), null, new Guid("0198ae00-0000-7000-8000-000000000004"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Okrągły owoc jabłoni o cienkiej skórce.", new Guid("0198ac50-0000-7000-8000-000000000001"), new Guid("0198ae10-0000-7000-8000-000000000007"), null, "Published", null, "Catalog" }
                });

            migrationBuilder.InsertData(
                schema: "vocabulary",
                table: "Pronunciations",
                columns: new[] { "Id", "AudioAssetReference", "LexemeId", "Region", "Scheme", "Value" },
                values: new object[,]
                {
                    { new Guid("0198ae40-0000-7000-8000-000000000001"), null, new Guid("0198ae10-0000-7000-8000-000000000001"), "en", "IPA", "/bæŋk/" },
                    { new Guid("0198ae40-0000-7000-8000-000000000002"), null, new Guid("0198ae10-0000-7000-8000-000000000004"), "en", "IPA", "/rʌn/" },
                    { new Guid("0198ae40-0000-7000-8000-000000000003"), null, new Guid("0198ae10-0000-7000-8000-000000000006"), "en", "IPA", "/ˈæp.əl/" },
                    { new Guid("0198ae40-0000-7000-8000-000000000004"), null, new Guid("0198ae10-0000-7000-8000-000000000007"), "pl", "IPA", "/ˈjap.kɔ/" }
                });

            migrationBuilder.InsertData(
                schema: "vocabulary",
                table: "WordForms",
                columns: new[] { "Id", "Form", "GrammarTags", "LexemeId", "NormalizedForm" },
                values: new object[,]
                {
                    { new Guid("0198ae30-0000-7000-8000-000000000001"), "banks", "plural", new Guid("0198ae10-0000-7000-8000-000000000001"), "banks" },
                    { new Guid("0198ae30-0000-7000-8000-000000000002"), "ran", "past", new Guid("0198ae10-0000-7000-8000-000000000004"), "ran" },
                    { new Guid("0198ae30-0000-7000-8000-000000000003"), "running", "present-participle", new Guid("0198ae10-0000-7000-8000-000000000004"), "running" },
                    { new Guid("0198ae30-0000-7000-8000-000000000004"), "apples", "plural", new Guid("0198ae10-0000-7000-8000-000000000006"), "apples" },
                    { new Guid("0198ae30-0000-7000-8000-000000000005"), "jabłka", "genitive-singular;nominative-plural", new Guid("0198ae10-0000-7000-8000-000000000007"), "jabłka" }
                });

            migrationBuilder.InsertData(
                schema: "vocabulary",
                table: "ExampleUsages",
                columns: new[] { "ExampleSentenceId", "LexemeSenseId", "HighlightLength", "HighlightStart" },
                values: new object[,]
                {
                    { new Guid("0198ae60-0000-7000-8000-000000000001"), new Guid("0198ae20-0000-7000-8000-000000000001"), 4, 15 },
                    { new Guid("0198ae60-0000-7000-8000-000000000002"), new Guid("0198ae20-0000-7000-8000-000000000002"), 4, 20 },
                    { new Guid("0198ae60-0000-7000-8000-000000000003"), new Guid("0198ae20-0000-7000-8000-000000000004"), 6, 14 },
                    { new Guid("0198ae60-0000-7000-8000-000000000004"), new Guid("0198ae20-0000-7000-8000-000000000005"), 3, 2 },
                    { new Guid("0198ae60-0000-7000-8000-000000000005"), new Guid("0198ae20-0000-7000-8000-000000000006"), 4, 5 },
                    { new Guid("0198ae60-0000-7000-8000-000000000006"), new Guid("0198ae20-0000-7000-8000-000000000007"), 5, 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Concepts_Key",
                schema: "vocabulary",
                table: "Concepts",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Concepts_OwnerUserId_Visibility",
                schema: "vocabulary",
                table: "Concepts",
                columns: new[] { "OwnerUserId", "Visibility" });

            migrationBuilder.CreateIndex(
                name: "IX_ExampleSentences_LanguageId",
                schema: "vocabulary",
                table: "ExampleSentences",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_ExampleSentences_OwnerUserId",
                schema: "vocabulary",
                table: "ExampleSentences",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExampleUsages_ExampleSentenceId",
                schema: "vocabulary",
                table: "ExampleUsages",
                column: "ExampleSentenceId");

            migrationBuilder.CreateIndex(
                name: "IX_LexemeFeatures_LexemeId_Key_Value",
                schema: "vocabulary",
                table: "LexemeFeatures",
                columns: new[] { "LexemeId", "Key", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lexemes_FrequencyRank",
                schema: "vocabulary",
                table: "Lexemes",
                column: "FrequencyRank");

            migrationBuilder.CreateIndex(
                name: "IX_Lexemes_LanguageId_NormalizedLemma",
                schema: "vocabulary",
                table: "Lexemes",
                columns: new[] { "LanguageId", "NormalizedLemma" });

            migrationBuilder.CreateIndex(
                name: "IX_Lexemes_LanguageId_PartOfSpeech_NormalizedLemma",
                schema: "vocabulary",
                table: "Lexemes",
                columns: new[] { "LanguageId", "PartOfSpeech", "NormalizedLemma" });

            migrationBuilder.CreateIndex(
                name: "IX_Lexemes_OwnerUserId_LanguageId_PartOfSpeech_NormalizedLemma",
                schema: "vocabulary",
                table: "Lexemes",
                columns: new[] { "OwnerUserId", "LanguageId", "PartOfSpeech", "NormalizedLemma" },
                unique: true,
                filter: "\"OwnerUserId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lexemes_PublicationStatus_CefrLevel",
                schema: "vocabulary",
                table: "Lexemes",
                columns: new[] { "PublicationStatus", "CefrLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_LexemeSenses_ConceptId",
                schema: "vocabulary",
                table: "LexemeSenses",
                column: "ConceptId");

            migrationBuilder.CreateIndex(
                name: "IX_LexemeSenses_DefinitionLanguageId",
                schema: "vocabulary",
                table: "LexemeSenses",
                column: "DefinitionLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_LexemeSenses_LexemeId_ConceptId",
                schema: "vocabulary",
                table: "LexemeSenses",
                columns: new[] { "LexemeId", "ConceptId" });

            migrationBuilder.CreateIndex(
                name: "IX_LexemeSenses_OwnerUserId",
                schema: "vocabulary",
                table: "LexemeSenses",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Pronunciations_LexemeId",
                schema: "vocabulary",
                table: "Pronunciations",
                column: "LexemeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVocabularyItems_LexemeSenseId",
                schema: "vocabulary",
                table: "UserVocabularyItems",
                column: "LexemeSenseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVocabularyItems_UserId_LexemeSenseId",
                schema: "vocabulary",
                table: "UserVocabularyItems",
                columns: new[] { "UserId", "LexemeSenseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserVocabularyItems_UserId_Status_StatusChangedAt",
                schema: "vocabulary",
                table: "UserVocabularyItems",
                columns: new[] { "UserId", "Status", "StatusChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WordForms_LexemeId_NormalizedForm",
                schema: "vocabulary",
                table: "WordForms",
                columns: new[] { "LexemeId", "NormalizedForm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExampleUsages",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "LexemeFeatures",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "Pronunciations",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "UserVocabularyItems",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "WordForms",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "ExampleSentences",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "LexemeSenses",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "Concepts",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "Lexemes",
                schema: "vocabulary");
        }
    }
}
