using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_StudentGroup_CallSupervisor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FypCallId",
                table: "StudentGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StudentGroups_FypCallId",
                table: "StudentGroups",
                column: "FypCallId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGroups_FypCalls_FypCallId",
                table: "StudentGroups",
                column: "FypCallId",
                principalTable: "FypCalls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentGroups_FypCalls_FypCallId",
                table: "StudentGroups");

            migrationBuilder.DropIndex(
                name: "IX_StudentGroups_FypCallId",
                table: "StudentGroups");

            migrationBuilder.DropColumn(
                name: "FypCallId",
                table: "StudentGroups");
        }
    }
}
