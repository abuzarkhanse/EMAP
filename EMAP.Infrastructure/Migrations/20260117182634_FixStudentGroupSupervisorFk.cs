using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixStudentGroupSupervisorFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FypChapterSubmissions_StudentGroups_GroupId",
                table: "FypChapterSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentGroups_FypSupervisors_SupervisorId",
                table: "StudentGroups");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "FypSupervisors",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "FypSupervisors",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SupervisorId",
                table: "FypChapterSubmissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_FypSupervisors_UserId",
                table: "FypSupervisors",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FypSupervisors_UserId1",
                table: "FypSupervisors",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_FypChapterSubmissions_StudentGroups_GroupId",
                table: "FypChapterSubmissions",
                column: "GroupId",
                principalTable: "StudentGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FypSupervisors_AspNetUsers_UserId",
                table: "FypSupervisors",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FypSupervisors_AspNetUsers_UserId1",
                table: "FypSupervisors",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGroups_FypSupervisors_SupervisorId",
                table: "StudentGroups",
                column: "SupervisorId",
                principalTable: "FypSupervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FypChapterSubmissions_StudentGroups_GroupId",
                table: "FypChapterSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_FypSupervisors_AspNetUsers_UserId",
                table: "FypSupervisors");

            migrationBuilder.DropForeignKey(
                name: "FK_FypSupervisors_AspNetUsers_UserId1",
                table: "FypSupervisors");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentGroups_FypSupervisors_SupervisorId",
                table: "StudentGroups");

            migrationBuilder.DropIndex(
                name: "IX_FypSupervisors_UserId",
                table: "FypSupervisors");

            migrationBuilder.DropIndex(
                name: "IX_FypSupervisors_UserId1",
                table: "FypSupervisors");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FypSupervisors");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "FypSupervisors");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "FypChapterSubmissions");

            migrationBuilder.AddForeignKey(
                name: "FK_FypChapterSubmissions_StudentGroups_GroupId",
                table: "FypChapterSubmissions",
                column: "GroupId",
                principalTable: "StudentGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGroups_FypSupervisors_SupervisorId",
                table: "StudentGroups",
                column: "SupervisorId",
                principalTable: "FypSupervisors",
                principalColumn: "Id");
        }
    }
}
