using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable IDE0161 // EF migration scaffolding uses block-scoped namespaces
#pragma warning disable CA1861 // EF migration scaffolding uses repeated array literals

namespace Spracher.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseSetsAndTranslation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExerciseSetItemId",
                schema: "exercises",
                table: "ExerciseAttempts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExerciseSets",
                schema: "exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseSets", x => x.Id);
                    table.CheckConstraint("CK_ExerciseSets_Publication", "(\"Status\" = 'Draft' AND \"PublishedAt\" IS NULL) OR (\"Status\" IN ('Published', 'Archived') AND \"PublishedAt\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ExerciseSets_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalSchema: "iam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseSetItems",
                schema: "exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseSetItems", x => x.Id);
                    table.CheckConstraint("CK_ExerciseSetItems_Position", "\"Position\" > 0");
                    table.ForeignKey(
                        name: "FK_ExerciseSetItems_ExerciseSets_ExerciseSetId",
                        column: x => x.ExerciseSetId,
                        principalSchema: "exercises",
                        principalTable: "ExerciseSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseSetItems_ExerciseVersions_ExerciseVersionId",
                        column: x => x.ExerciseVersionId,
                        principalSchema: "exercises",
                        principalTable: "ExerciseVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "exercises",
                table: "ExerciseDefinitions",
                columns: new[] { "Id", "ArchivedAt", "CreatedAt", "Description", "OwnerUserId", "Title", "TypeKey" },
                values: new object[] { new Guid("0198b100-0000-7000-8000-000000000003"), null, new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Translate a short Present Simple sentence into Polish.", null, "Present Simple: translate a sentence", "translation" });

            migrationBuilder.InsertData(
                schema: "exercises",
                table: "ExerciseSets",
                columns: new[] { "Id", "CreatedAt", "Description", "OwnerUserId", "PublishedAt", "Status", "Title" },
                values: new object[] { new Guid("0198b120-0000-7000-8000-000000000001"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Three short exercises covering the third-person singular.", null, new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Published", "Present Simple: quick practice" });

            migrationBuilder.UpdateData(
                schema: "exercises",
                table: "ExerciseVersions",
                keyColumn: "Id",
                keyValue: new Guid("0198b110-0000-7000-8000-000000000002"),
                column: "DefinitionJson",
                value: "{\r\n  \"segments\": [\r\n    { \"kind\": \"text\", \"text\": \"She \", \"blankId\": null },\r\n    { \"kind\": \"blank\", \"text\": null, \"blankId\": \"verb\" },\r\n    { \"kind\": \"text\", \"text\": \" to school every day.\", \"blankId\": null }\r\n  ],\r\n  \"answers\": { \"verb\": [\"goes\"] },\r\n  \"caseSensitive\": false,\r\n  \"trimWhitespace\": true,\r\n  \"points\": 10,\r\n  \"correctFeedback\": \"Correct — go changes to goes with she.\",\r\n  \"incorrectFeedback\": \"Use the third-person singular form: goes.\"\r\n}");

            migrationBuilder.InsertData(
                schema: "exercises",
                table: "ExerciseSetItems",
                columns: new[] { "Id", "ExerciseSetId", "ExerciseVersionId", "Position" },
                values: new object[,]
                {
                    { new Guid("0198b130-0000-7000-8000-000000000001"), new Guid("0198b120-0000-7000-8000-000000000001"), new Guid("0198b110-0000-7000-8000-000000000001"), 1 },
                    { new Guid("0198b130-0000-7000-8000-000000000002"), new Guid("0198b120-0000-7000-8000-000000000001"), new Guid("0198b110-0000-7000-8000-000000000002"), 2 }
                });

            migrationBuilder.InsertData(
                schema: "exercises",
                table: "ExerciseVersions",
                columns: new[] { "Id", "CreatedAt", "DefinitionJson", "ExerciseDefinitionId", "Prompt", "PublishedAt", "SchemaVersion", "Status", "VersionNumber" },
                values: new object[] { new Guid("0198b110-0000-7000-8000-000000000003"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{\r\n  \"sourceText\": \"She goes to school every day.\",\r\n  \"sourceLanguageCode\": \"en\",\r\n  \"targetLanguageCode\": \"pl\",\r\n  \"acceptedAnswers\": [\r\n    \"Ona chodzi do szkoły codziennie.\",\r\n    \"Ona codziennie chodzi do szkoły.\"\r\n  ],\r\n  \"caseSensitive\": false,\r\n  \"trimWhitespace\": true,\r\n  \"collapseWhitespace\": true,\r\n  \"ignoreTerminalPunctuation\": true,\r\n  \"points\": 10,\r\n  \"correctFeedback\": \"Correct — both natural word orders are accepted.\",\r\n  \"incorrectFeedback\": \"Check the verb form and the phrase every day.\"\r\n}", new Guid("0198b100-0000-7000-8000-000000000003"), "Translate the sentence into Polish.", new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, "Published", 1 });

            migrationBuilder.InsertData(
                schema: "exercises",
                table: "ExerciseSetItems",
                columns: new[] { "Id", "ExerciseSetId", "ExerciseVersionId", "Position" },
                values: new object[] { new Guid("0198b130-0000-7000-8000-000000000003"), new Guid("0198b120-0000-7000-8000-000000000001"), new Guid("0198b110-0000-7000-8000-000000000003"), 3 });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseAttempts_ExerciseSetItemId",
                schema: "exercises",
                table: "ExerciseAttempts",
                column: "ExerciseSetItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSetItems_ExerciseSetId_ExerciseVersionId",
                schema: "exercises",
                table: "ExerciseSetItems",
                columns: new[] { "ExerciseSetId", "ExerciseVersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSetItems_ExerciseSetId_Position",
                schema: "exercises",
                table: "ExerciseSetItems",
                columns: new[] { "ExerciseSetId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSetItems_ExerciseVersionId",
                schema: "exercises",
                table: "ExerciseSetItems",
                column: "ExerciseVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSets_OwnerUserId",
                schema: "exercises",
                table: "ExerciseSets",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSets_Status_PublishedAt",
                schema: "exercises",
                table: "ExerciseSets",
                columns: new[] { "Status", "PublishedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseAttempts_ExerciseSetItems_ExerciseSetItemId",
                schema: "exercises",
                table: "ExerciseAttempts",
                column: "ExerciseSetItemId",
                principalSchema: "exercises",
                principalTable: "ExerciseSetItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseAttempts_ExerciseSetItems_ExerciseSetItemId",
                schema: "exercises",
                table: "ExerciseAttempts");

            migrationBuilder.DropTable(
                name: "ExerciseSetItems",
                schema: "exercises");

            migrationBuilder.DropTable(
                name: "ExerciseSets",
                schema: "exercises");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseAttempts_ExerciseSetItemId",
                schema: "exercises",
                table: "ExerciseAttempts");

            migrationBuilder.DeleteData(
                schema: "exercises",
                table: "ExerciseVersions",
                keyColumn: "Id",
                keyValue: new Guid("0198b110-0000-7000-8000-000000000003"));

            migrationBuilder.DeleteData(
                schema: "exercises",
                table: "ExerciseDefinitions",
                keyColumn: "Id",
                keyValue: new Guid("0198b100-0000-7000-8000-000000000003"));

            migrationBuilder.DropColumn(
                name: "ExerciseSetItemId",
                schema: "exercises",
                table: "ExerciseAttempts");

            migrationBuilder.UpdateData(
                schema: "exercises",
                table: "ExerciseVersions",
                keyColumn: "Id",
                keyValue: new Guid("0198b110-0000-7000-8000-000000000002"),
                column: "DefinitionJson",
                value: "{\n  \"segments\": [\n    { \"kind\": \"text\", \"text\": \"She \", \"blankId\": null },\n    { \"kind\": \"blank\", \"text\": null, \"blankId\": \"verb\" },\n    { \"kind\": \"text\", \"text\": \" to school every day.\", \"blankId\": null }\n  ],\n  \"answers\": { \"verb\": [\"goes\"] },\n  \"caseSensitive\": false,\n  \"trimWhitespace\": true,\n  \"points\": 10,\n  \"correctFeedback\": \"Correct — go changes to goes with she.\",\n  \"incorrectFeedback\": \"Use the third-person singular form: goes.\"\n}");
        }
    }
}
