using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "StudentGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "FypSupervisors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "FypCalls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentGroups_DepartmentId",
                table: "StudentGroups",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FypSupervisors_DepartmentId",
                table: "FypSupervisors",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FypCalls_DepartmentId",
                table: "FypCalls",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DepartmentId",
                table: "AspNetUsers",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Code",
                table: "Departments",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Departments_DepartmentId",
                table: "AspNetUsers",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FypCalls_Departments_DepartmentId",
                table: "FypCalls",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FypSupervisors_Departments_DepartmentId",
                table: "FypSupervisors",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGroups_Departments_DepartmentId",
                table: "StudentGroups",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Departments_DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_FypCalls_Departments_DepartmentId",
                table: "FypCalls");

            migrationBuilder.DropForeignKey(
                name: "FK_FypSupervisors_Departments_DepartmentId",
                table: "FypSupervisors");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentGroups_Departments_DepartmentId",
                table: "StudentGroups");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_StudentGroups_DepartmentId",
                table: "StudentGroups");

            migrationBuilder.DropIndex(
                name: "IX_FypSupervisors_DepartmentId",
                table: "FypSupervisors");

            migrationBuilder.DropIndex(
                name: "IX_FypCalls_DepartmentId",
                table: "FypCalls");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "StudentGroups");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "FypSupervisors");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "FypCalls");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
