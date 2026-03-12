using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EMAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFypStagesMilestonesEvaluations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentStage",
                table: "StudentGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MilestoneId",
                table: "FypChapterSubmissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Stage",
                table: "FypChapterSubmissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FypEvaluationCriteria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluationType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MaxMarks = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FypEvaluationCriteria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FypMilestones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Stage = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ChapterNumber = table.Column<int>(type: "int", nullable: true),
                    IsOptional = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FypMilestones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FypEvaluations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentGroupId = table.Column<int>(type: "int", nullable: false),
                    MilestoneId = table.Column<int>(type: "int", nullable: false),
                    EvaluatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalMarks = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsSubmitted = table.Column<bool>(type: "bit", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FypEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FypEvaluations_FypMilestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "FypMilestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FypEvaluations_StudentGroups_StudentGroupId",
                        column: x => x.StudentGroupId,
                        principalTable: "StudentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FypEvaluationScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluationId = table.Column<int>(type: "int", nullable: false),
                    CriterionId = table.Column<int>(type: "int", nullable: false),
                    AwardedMarks = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FypEvaluationScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FypEvaluationScores_FypEvaluationCriteria_CriterionId",
                        column: x => x.CriterionId,
                        principalTable: "FypEvaluationCriteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FypEvaluationScores_FypEvaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "FypEvaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "FypEvaluationCriteria",
                columns: new[] { "Id", "Description", "DisplayOrder", "EvaluationType", "IsActive", "MaxMarks", "Title" },
                values: new object[,]
                {
                    { 1, null, 1, 2, true, 10m, "Problem Understanding" },
                    { 2, null, 2, 2, true, 10m, "Progress and Methodology" },
                    { 3, null, 3, 2, true, 10m, "Documentation" },
                    { 4, null, 4, 2, true, 10m, "Presentation" },
                    { 5, null, 1, 3, true, 10m, "Implementation Progress" },
                    { 6, null, 2, 3, true, 10m, "Documentation Quality" },
                    { 7, null, 3, 3, true, 10m, "Presentation and Readiness" },
                    { 8, null, 1, 4, true, 20m, "Final Report" },
                    { 9, null, 2, 4, true, 20m, "Implementation" },
                    { 10, null, 3, 4, true, 10m, "Presentation" },
                    { 11, null, 4, 4, true, 10m, "Viva / Question Answer" }
                });

            migrationBuilder.InsertData(
                table: "FypMilestones",
                columns: new[] { "Id", "ChapterNumber", "Description", "DisplayOrder", "DueDate", "IsActive", "IsOptional", "Stage", "Title", "Type" },
                values: new object[,]
                {
                    { 1, 1, null, 1, null, true, false, 1, "FYP-1 Chapter 1", 1 },
                    { 2, 2, null, 2, null, true, false, 1, "FYP-1 Chapter 2", 1 },
                    { 3, 3, null, 3, null, true, false, 1, "FYP-1 Chapter 3", 1 },
                    { 4, null, null, 4, null, true, false, 1, "FYP-1 Mid Evaluation", 2 },
                    { 5, null, null, 5, null, true, false, 1, "FYP-1 Final Evaluation", 4 },
                    { 6, 1, null, 1, null, true, false, 2, "FYP-2 Chapter 1", 1 },
                    { 7, 2, null, 2, null, true, false, 2, "FYP-2 Chapter 2", 1 },
                    { 8, 3, null, 3, null, true, false, 2, "FYP-2 Chapter 3", 1 },
                    { 9, null, null, 4, null, true, false, 2, "FYP-2 Mid Evaluation", 2 },
                    { 10, null, null, 5, null, true, true, 2, "FYP-2 Pre-Final Evaluation", 3 },
                    { 11, null, null, 6, null, true, false, 2, "FYP-2 Final Evaluation", 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FypChapterSubmissions_MilestoneId",
                table: "FypChapterSubmissions",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_FypEvaluationCriteria_EvaluationType_DisplayOrder",
                table: "FypEvaluationCriteria",
                columns: new[] { "EvaluationType", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_FypEvaluations_MilestoneId",
                table: "FypEvaluations",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_FypEvaluations_StudentGroupId",
                table: "FypEvaluations",
                column: "StudentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FypEvaluationScores_CriterionId",
                table: "FypEvaluationScores",
                column: "CriterionId");

            migrationBuilder.CreateIndex(
                name: "IX_FypEvaluationScores_EvaluationId",
                table: "FypEvaluationScores",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_FypMilestones_Stage_Type_ChapterNumber_DisplayOrder",
                table: "FypMilestones",
                columns: new[] { "Stage", "Type", "ChapterNumber", "DisplayOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_FypChapterSubmissions_FypMilestones_MilestoneId",
                table: "FypChapterSubmissions",
                column: "MilestoneId",
                principalTable: "FypMilestones",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FypChapterSubmissions_FypMilestones_MilestoneId",
                table: "FypChapterSubmissions");

            migrationBuilder.DropTable(
                name: "FypEvaluationScores");

            migrationBuilder.DropTable(
                name: "FypEvaluationCriteria");

            migrationBuilder.DropTable(
                name: "FypEvaluations");

            migrationBuilder.DropTable(
                name: "FypMilestones");

            migrationBuilder.DropIndex(
                name: "IX_FypChapterSubmissions_MilestoneId",
                table: "FypChapterSubmissions");

            migrationBuilder.DropColumn(
                name: "CurrentStage",
                table: "StudentGroups");

            migrationBuilder.DropColumn(
                name: "MilestoneId",
                table: "FypChapterSubmissions");

            migrationBuilder.DropColumn(
                name: "Stage",
                table: "FypChapterSubmissions");
        }
    }
}
