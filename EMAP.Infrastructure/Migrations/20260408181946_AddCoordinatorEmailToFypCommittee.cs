using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoordinatorEmailToFypCommittee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoordinatorUserId",
                table: "FypCommittees");

            migrationBuilder.AddColumn<string>(
                name: "CoordinatorEmail",
                table: "FypCommittees",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoordinatorEmail",
                table: "FypCommittees");

            migrationBuilder.AddColumn<string>(
                name: "CoordinatorUserId",
                table: "FypCommittees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
