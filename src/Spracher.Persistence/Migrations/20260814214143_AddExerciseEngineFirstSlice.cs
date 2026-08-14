using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable IDE0161 // EF migration scaffolding uses block-scoped namespaces
#pragma warning disable CA1861 // EF migration scaffolding uses repeated array literals

namespace Spracher.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseEngineFirstSlice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "exercises");

            migrationBuilder.CreateTable(
                name: "ExerciseDefinitions",
                schema: "exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ArchivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseVersions",
                schema: "exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    SchemaVersion = table.Column<int>(type: "integer", nullable: false),
                    Prompt = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DefinitionJson = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseVersions", x => x.Id);
                    table.CheckConstraint("CK_ExerciseVersions_Schema", "\"SchemaVersion\" > 0");
                    table.CheckConstraint("CK_ExerciseVersions_Version", "\"VersionNumber\" > 0");
                    table.ForeignKey(
                        name: "FK_ExerciseVersions_ExerciseDefinitions_ExerciseDefinitionId",
                        column: x => x.ExerciseDefinitionId,
                        principalSchema: "exercises",
                        principalTable: "ExerciseDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseAttempts",
                schema: "exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AwardedPoints = table.Column<int>(type: "integer", nullable: true),
                    MaxPoints = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseAttempts", x => x.Id);
                    table.CheckConstraint("CK_ExerciseAttempts_Score", "(\"AwardedPoints\" IS NULL AND \"MaxPoints\" IS NULL) OR (\"AwardedPoints\" >= 0 AND \"MaxPoints\" > 0 AND \"AwardedPoints\" <= \"MaxPoints\")");
                    table.ForeignKey(
                        name: "FK_ExerciseAttempts_ExerciseVersions_ExerciseVersionId",
                        column: x => x.ExerciseVersionId,
                        principalSchema: "exercises",
                        principalTable: "ExerciseVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExerciseAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "iam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseSubmissions",
                schema: "exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseJson = table.Column<string>(type: "jsonb", nullable: false),
                    GradingJson = table.Column<string>(type: "jsonb", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    AwardedPoints = table.Column<int>(type: "integer", nullable: false),
                    MaxPoints = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseSubmissions", x => x.Id);
                    table.CheckConstraint("CK_ExerciseSubmissions_Score", "\"AwardedPoints\" >= 0 AND \"MaxPoints\" > 0 AND \"AwardedPoints\" <= \"MaxPoints\"");
                    table.ForeignKey(
                        name: "FK_ExerciseSubmissions_ExerciseAttempts_AttemptId",
                        column: x => x.AttemptId,
                        principalSchema: "exercises",
                        principalTable: "ExerciseAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "exercises",
                table: "ExerciseDefinitions",
                columns: new[] { "Id", "ArchivedAt", "CreatedAt", "Description", "Title", "TypeKey" },
                values: new object[] { new Guid("0198b100-0000-7000-8000-000000000001"), null, new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Choose the correct verb form for he, she or it.", "Present Simple: third person", "multiple-choice" });

            migrationBuilder.InsertData(
                schema: "exercises",
                table: "ExerciseVersions",
                columns: new[] { "Id", "CreatedAt", "DefinitionJson", "ExerciseDefinitionId", "Prompt", "PublishedAt", "SchemaVersion", "Status", "VersionNumber" },
                values: new object[] { new Guid("0198b110-0000-7000-8000-000000000001"), new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{\n  \"options\": [\n    { \"id\": \"work\", \"text\": \"She work in a bank.\" },\n    { \"id\": \"works\", \"text\": \"She works in a bank.\" },\n    { \"id\": \"working\", \"text\": \"She working in a bank.\" }\n  ],\n  \"correctOptionIds\": [\"works\"],\n  \"points\": 10,\n  \"correctFeedback\": \"Exactly — use -s with she in the Present Simple.\",\n  \"incorrectFeedback\": \"Remember: in the Present Simple, he/she/it takes -s.\"\n}", new Guid("0198b100-0000-7000-8000-000000000001"), "Choose the correct sentence.", new DateTimeOffset(new DateTime(2026, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, "Published", 1 });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseAttempts_ExerciseVersionId",
                schema: "exercises",
                table: "ExerciseAttempts",
                column: "ExerciseVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseAttempts_UserId_StartedAt",
                schema: "exercises",
                table: "ExerciseAttempts",
                columns: new[] { "UserId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseDefinitions_TypeKey_ArchivedAt",
                schema: "exercises",
                table: "ExerciseDefinitions",
                columns: new[] { "TypeKey", "ArchivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSubmissions_AttemptId",
                schema: "exercises",
                table: "ExerciseSubmissions",
                column: "AttemptId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseVersions_ExerciseDefinitionId_Status_VersionNumber",
                schema: "exercises",
                table: "ExerciseVersions",
                columns: new[] { "ExerciseDefinitionId", "Status", "VersionNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseVersions_ExerciseDefinitionId_VersionNumber",
                schema: "exercises",
                table: "ExerciseVersions",
                columns: new[] { "ExerciseDefinitionId", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseSubmissions",
                schema: "exercises");

            migrationBuilder.DropTable(
                name: "ExerciseAttempts",
                schema: "exercises");

            migrationBuilder.DropTable(
                name: "ExerciseVersions",
                schema: "exercises");

            migrationBuilder.DropTable(
                name: "ExerciseDefinitions",
                schema: "exercises");
        }
    }
}
