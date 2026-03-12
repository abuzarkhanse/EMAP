using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFypEvaluationSchedulingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommitteeMembers",
                table: "FypEvaluations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "FypEvaluations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublishedToStudent",
                table: "FypEvaluations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowCommitteeToStudent",
                table: "FypEvaluations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Venue",
                table: "FypEvaluations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommitteeMembers",
                table: "FypEvaluations");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "FypEvaluations");

            migrationBuilder.DropColumn(
                name: "IsPublishedToStudent",
                table: "FypEvaluations");

            migrationBuilder.DropColumn(
                name: "ShowCommitteeToStudent",
                table: "FypEvaluations");

            migrationBuilder.DropColumn(
                name: "Venue",
                table: "FypEvaluations");
        }
    }
}
