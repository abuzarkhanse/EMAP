using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFypSupervisorsAndGroupStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "StudentGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                table: "StudentGroups",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FypSupervisors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FieldOfExpertise = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxSlots = table.Column<int>(type: "int", nullable: false),
                    CurrentSlots = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FypSupervisors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentGroups_SupervisorId",
                table: "StudentGroups",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGroups_FypSupervisors_SupervisorId",
                table: "StudentGroups",
                column: "SupervisorId",
                principalTable: "FypSupervisors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentGroups_FypSupervisors_SupervisorId",
                table: "StudentGroups");

            migrationBuilder.DropTable(
                name: "FypSupervisors");

            migrationBuilder.DropIndex(
                name: "IX_StudentGroups_SupervisorId",
                table: "StudentGroups");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "StudentGroups");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "StudentGroups");
        }
    }
}
