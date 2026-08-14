using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable IDE0161 // EF migration scaffolding uses block-scoped namespaces
#pragma warning disable CA1861 // EF migration scaffolding uses repeated array literals

namespace Spracher.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVocabularyListsAndCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VocabularyCategories",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Color = table.Column<string>(type: "character(7)", fixedLength: true, maxLength: 7, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VocabularyCategories_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalSchema: "iam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VocabularyLists",
                schema: "vocabulary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VocabularyLists_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalSchema: "iam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserVocabularyItemCategories",
                schema: "vocabulary",
                columns: table => new
                {
                    UserVocabularyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    VocabularyCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVocabularyItemCategories", x => new { x.UserVocabularyItemId, x.VocabularyCategoryId });
                    table.ForeignKey(
                        name: "FK_UserVocabularyItemCategories_UserVocabularyItems_UserVocabu~",
                        column: x => x.UserVocabularyItemId,
                        principalSchema: "vocabulary",
                        principalTable: "UserVocabularyItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserVocabularyItemCategories_VocabularyCategories_Vocabular~",
                        column: x => x.VocabularyCategoryId,
                        principalSchema: "vocabulary",
                        principalTable: "VocabularyCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VocabularyListItems",
                schema: "vocabulary",
                columns: table => new
                {
                    VocabularyListId = table.Column<Guid>(type: "uuid", nullable: false),
                    LexemeSenseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AddedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyListItems", x => new { x.VocabularyListId, x.LexemeSenseId });
                    table.CheckConstraint("CK_VocabularyListItems_Position", "\"Position\" >= 0");
                    table.ForeignKey(
                        name: "FK_VocabularyListItems_LexemeSenses_LexemeSenseId",
                        column: x => x.LexemeSenseId,
                        principalSchema: "vocabulary",
                        principalTable: "LexemeSenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VocabularyListItems_VocabularyLists_VocabularyListId",
                        column: x => x.VocabularyListId,
                        principalSchema: "vocabulary",
                        principalTable: "VocabularyLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserVocabularyItemCategories_VocabularyCategoryId",
                schema: "vocabulary",
                table: "UserVocabularyItemCategories",
                column: "VocabularyCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyCategories_OwnerUserId_NormalizedName",
                schema: "vocabulary",
                table: "VocabularyCategories",
                columns: new[] { "OwnerUserId", "NormalizedName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyListItems_LexemeSenseId",
                schema: "vocabulary",
                table: "VocabularyListItems",
                column: "LexemeSenseId");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyListItems_VocabularyListId_Position",
                schema: "vocabulary",
                table: "VocabularyListItems",
                columns: new[] { "VocabularyListId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyLists_OwnerUserId_NormalizedName",
                schema: "vocabulary",
                table: "VocabularyLists",
                columns: new[] { "OwnerUserId", "NormalizedName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserVocabularyItemCategories",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "VocabularyListItems",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "VocabularyCategories",
                schema: "vocabulary");

            migrationBuilder.DropTable(
                name: "VocabularyLists",
                schema: "vocabulary");
        }
    }
}
