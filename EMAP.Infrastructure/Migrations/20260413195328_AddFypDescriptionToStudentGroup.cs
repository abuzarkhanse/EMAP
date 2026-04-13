using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFypDescriptionToStudentGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FypDescription",
                table: "StudentGroups",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FypDescription",
                table: "StudentGroups");
        }
    }
}
