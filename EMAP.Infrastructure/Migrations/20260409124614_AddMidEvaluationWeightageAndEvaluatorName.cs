using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EMAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMidEvaluationWeightageAndEvaluatorName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EvaluatorName",
                table: "FypEvaluations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightagePercent",
                table: "FypEvaluations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightedMarks",
                table: "FypEvaluations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.InsertData(
                table: "FypEvaluationCriteria",
                columns: new[] { "Id", "Description", "DisplayOrder", "EvaluationType", "IsActive", "MaxMarks", "Title" },
                values: new object[,]
                {
                    { 3001, "Understanding of project domain, concepts, and technical knowledge.", 1, 2, true, 5m, "Content & Knowledge" },
                    { 3002, "Clarity and depth of problem understanding and analysis.", 2, 2, true, 5m, "Problem Analysis" },
                    { 3003, "Research effort, literature review, and technical investigation.", 3, 2, true, 5m, "Investigation" },
                    { 3004, "System design, architecture, modeling, and proposed solution quality.", 4, 2, true, 5m, "Design of Solution" },
                    { 3005, "Implementation progress and effective use of tools/frameworks.", 5, 2, true, 5m, "Progress & Tool Usage" },
                    { 3006, "Planning, task management, teamwork, and timeline handling.", 6, 2, true, 5m, "Project Management" },
                    { 3007, "Presentation quality, flow, communication, and confidence.", 7, 2, true, 5m, "Presentation" },
                    { 3008, "Answers during questioning, conceptual clarity, and confidence.", 8, 2, true, 5m, "Viva" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "FypEvaluationCriteria",
                keyColumn: "Id",
                keyValue: 3001);

            migrationBuilder.DeleteData(
                table: "FypEvaluationCriteria",
                keyColumn: "Id",
                keyValue: 3002);

            migrationBuilder.DeleteData(
                table: "FypEvaluationCriteria",
                keyColumn: "Id",
                keyValue: 3003);

            migrationBuilder.DeleteData(
                table: "FypEvaluationCriteria",
                keyColumn: "Id",
                keyValue: 3004);

            migrationBuilder.DeleteData(
                table: "FypEvaluationCriteria",
                keyColumn: "Id",
                keyValue: 3005);

            migrationBuilder.DeleteData(
                table: "FypEvaluationCriteria",
                keyColumn: "Id",
                keyValue: 3006);

            migrationBuilder.DeleteData(
                table: "FypEvaluationCriteria",
                keyColumn: "Id",
                keyValue: 3007);

            migrationBuilder.DeleteData(
                table: "FypEvaluationCriteria",
                keyColumn: "Id",
                keyValue: 3008);

            migrationBuilder.DropColumn(
                name: "EvaluatorName",
                table: "FypEvaluations");

            migrationBuilder.DropColumn(
                name: "WeightagePercent",
                table: "FypEvaluations");

            migrationBuilder.DropColumn(
                name: "WeightedMarks",
                table: "FypEvaluations");
        }
    }
}
