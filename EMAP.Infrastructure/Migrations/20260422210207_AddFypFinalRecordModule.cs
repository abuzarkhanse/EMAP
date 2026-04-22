using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFypFinalRecordModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FypFinalRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentGroupId = table.Column<int>(type: "int", nullable: false),
                    FypCallId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    ProjectTitle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ProgramCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Batch = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SupervisorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FypDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsFypCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fyp1AverageMarks = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Fyp2AverageMarks = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FinalAverageMarks = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SubmittedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmittedToAdminAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CoordinatorRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessedByAdminAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdminRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FypFinalRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FypFinalRecords_StudentGroups_StudentGroupId",
                        column: x => x.StudentGroupId,
                        principalTable: "StudentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FypFinalRecordChapters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinalRecordId = table.Column<int>(type: "int", nullable: false),
                    Stage = table.Column<int>(type: "int", nullable: false),
                    ChapterType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FypFinalRecordChapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FypFinalRecordChapters_FypFinalRecords_FinalRecordId",
                        column: x => x.FinalRecordId,
                        principalTable: "FypFinalRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FypFinalRecordEvaluations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinalRecordId = table.Column<int>(type: "int", nullable: false),
                    Stage = table.Column<int>(type: "int", nullable: false),
                    EvaluationType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Venue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EvaluatorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalMarks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WeightagePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WeightedMarks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublishedToStudent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FypFinalRecordEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FypFinalRecordEvaluations_FypFinalRecords_FinalRecordId",
                        column: x => x.FinalRecordId,
                        principalTable: "FypFinalRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FypFinalRecordStudents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinalRecordId = table.Column<int>(type: "int", nullable: false),
                    StudentUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RegistrationNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RoleInGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FypFinalRecordStudents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FypFinalRecordStudents_FypFinalRecords_FinalRecordId",
                        column: x => x.FinalRecordId,
                        principalTable: "FypFinalRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FypFinalRecordChapters_FinalRecordId",
                table: "FypFinalRecordChapters",
                column: "FinalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_FypFinalRecordEvaluations_FinalRecordId",
                table: "FypFinalRecordEvaluations",
                column: "FinalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_FypFinalRecords_StudentGroupId",
                table: "FypFinalRecords",
                column: "StudentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FypFinalRecordStudents_FinalRecordId",
                table: "FypFinalRecordStudents",
                column: "FinalRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FypFinalRecordChapters");

            migrationBuilder.DropTable(
                name: "FypFinalRecordEvaluations");

            migrationBuilder.DropTable(
                name: "FypFinalRecordStudents");

            migrationBuilder.DropTable(
                name: "FypFinalRecords");
        }
    }
}
