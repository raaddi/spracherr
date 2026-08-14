using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable IDE0161 // EF migration scaffolding uses block-scoped namespaces
#pragma warning disable CA1861 // EF migration scaffolding uses repeated array literals

namespace Spracher.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeVocabularyPrefixSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lexemes_LanguageId_NormalizedLemma",
                schema: "vocabulary",
                table: "Lexemes");

            migrationBuilder.CreateIndex(
                name: "IX_Lexemes_LanguageId_NormalizedLemma",
                schema: "vocabulary",
                table: "Lexemes",
                columns: new[] { "LanguageId", "NormalizedLemma" })
                .Annotation("Npgsql:IndexOperators", new[] { "uuid_ops", "text_pattern_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lexemes_LanguageId_NormalizedLemma",
                schema: "vocabulary",
                table: "Lexemes");

            migrationBuilder.CreateIndex(
                name: "IX_Lexemes_LanguageId_NormalizedLemma",
                schema: "vocabulary",
                table: "Lexemes",
                columns: new[] { "LanguageId", "NormalizedLemma" });
        }
    }
}
