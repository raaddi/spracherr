using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable IDE0161 // EF migration scaffolding uses block-scoped namespaces
#pragma warning disable CA1861 // EF migration scaffolding uses repeated array literals

namespace Spracher.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseAuthoringAndFillInBlank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                schema: "exercises",
                table: "ExerciseDefinitions",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "exercises",
                table: "ExerciseDefinitions",
                keyColumn: "Id",
                keyValue: new Guid("0198b100-0000-7000-8000-000000000001"),
                column: "OwnerUserId",
                value: null);

            migrationBuilder.InsertData(
                schema: "exercises",
                table: "ExerciseDefinitions",
                columns: new[] { "Id", "ArchivedAt", "CreatedAt", "Description", "OwnerUserId", "Title", "TypeKey" },
                values: new object[] { new Guid("0198b100-0000-7000-8000-000000000002"), null, new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Complete the sentence with the correct third-person verb form.", null, "Present Simple: missing verb", "fill-in-blank" });

            migrationBuilder.UpdateData(
                schema: "exercises",
                table: "ExerciseVersions",
                keyColumn: "Id",
                keyValue: new Guid("0198b110-0000-7000-8000-000000000001"),
                column: "DefinitionJson",
                value: "{\r\n  \"options\": [\r\n    { \"id\": \"work\", \"text\": \"She work in a bank.\" },\r\n    { \"id\": \"works\", \"text\": \"She works in a bank.\" },\r\n    { \"id\": \"working\", \"text\": \"She working in a bank.\" }\r\n  ],\r\n  \"correctOptionIds\": [\"works\"],\r\n  \"points\": 10,\r\n  \"correctFeedback\": \"Exactly — use -s with she in the Present Simple.\",\r\n  \"incorrectFeedback\": \"Remember: in the Present Simple, he/she/it takes -s.\"\r\n}");

            migrationBuilder.InsertData(
                schema: "exercises",
                table: "ExerciseVersions",
                columns: new[] { "Id", "CreatedAt", "DefinitionJson", "ExerciseDefinitionId", "Prompt", "PublishedAt", "SchemaVersion", "Status", "VersionNumber" },
                values: new object[] { new Guid("0198b110-0000-7000-8000-000000000002"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{\n  \"segments\": [\n    { \"kind\": \"text\", \"text\": \"She \", \"blankId\": null },\n    { \"kind\": \"blank\", \"text\": null, \"blankId\": \"verb\" },\n    { \"kind\": \"text\", \"text\": \" to school every day.\", \"blankId\": null }\n  ],\n  \"answers\": { \"verb\": [\"goes\"] },\n  \"caseSensitive\": false,\n  \"trimWhitespace\": true,\n  \"points\": 10,\n  \"correctFeedback\": \"Correct — go changes to goes with she.\",\n  \"incorrectFeedback\": \"Use the third-person singular form: goes.\"\n}", new Guid("0198b100-0000-7000-8000-000000000002"), "Complete the missing word.", new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, "Published", 1 });

            migrationBuilder.CreateIndex(
                name: "UX_ExerciseVersions_OneDraftPerDefinition",
                schema: "exercises",
                table: "ExerciseVersions",
                column: "ExerciseDefinitionId",
                unique: true,
                filter: "\"Status\" = 'Draft'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ExerciseVersions_Publication",
                schema: "exercises",
                table: "ExerciseVersions",
                sql: "(\"Status\" = 'Draft' AND \"PublishedAt\" IS NULL) OR (\"Status\" IN ('Published', 'Archived') AND \"PublishedAt\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseDefinitions_OwnerUserId",
                schema: "exercises",
                table: "ExerciseDefinitions",
                column: "OwnerUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ExerciseAttempts_Lifecycle",
                schema: "exercises",
                table: "ExerciseAttempts",
                sql: "(\"Status\" = 'InProgress' AND \"CompletedAt\" IS NULL AND \"AwardedPoints\" IS NULL AND \"MaxPoints\" IS NULL) OR (\"Status\" = 'Completed' AND \"CompletedAt\" IS NOT NULL AND \"AwardedPoints\" IS NOT NULL AND \"MaxPoints\" IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseDefinitions_Users_OwnerUserId",
                schema: "exercises",
                table: "ExerciseDefinitions",
                column: "OwnerUserId",
                principalSchema: "iam",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseDefinitions_Users_OwnerUserId",
                schema: "exercises",
                table: "ExerciseDefinitions");

            migrationBuilder.DropIndex(
                name: "UX_ExerciseVersions_OneDraftPerDefinition",
                schema: "exercises",
                table: "ExerciseVersions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ExerciseVersions_Publication",
                schema: "exercises",
                table: "ExerciseVersions");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseDefinitions_OwnerUserId",
                schema: "exercises",
                table: "ExerciseDefinitions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ExerciseAttempts_Lifecycle",
                schema: "exercises",
                table: "ExerciseAttempts");

            migrationBuilder.DeleteData(
                schema: "exercises",
                table: "ExerciseVersions",
                keyColumn: "Id",
                keyValue: new Guid("0198b110-0000-7000-8000-000000000002"));

            migrationBuilder.DeleteData(
                schema: "exercises",
                table: "ExerciseDefinitions",
                keyColumn: "Id",
                keyValue: new Guid("0198b100-0000-7000-8000-000000000002"));

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                schema: "exercises",
                table: "ExerciseDefinitions");

            migrationBuilder.UpdateData(
                schema: "exercises",
                table: "ExerciseVersions",
                keyColumn: "Id",
                keyValue: new Guid("0198b110-0000-7000-8000-000000000001"),
                column: "DefinitionJson",
                value: "{\n  \"options\": [\n    { \"id\": \"work\", \"text\": \"She work in a bank.\" },\n    { \"id\": \"works\", \"text\": \"She works in a bank.\" },\n    { \"id\": \"working\", \"text\": \"She working in a bank.\" }\n  ],\n  \"correctOptionIds\": [\"works\"],\n  \"points\": 10,\n  \"correctFeedback\": \"Exactly — use -s with she in the Present Simple.\",\n  \"incorrectFeedback\": \"Remember: in the Present Simple, he/she/it takes -s.\"\n}");
        }
    }
}
