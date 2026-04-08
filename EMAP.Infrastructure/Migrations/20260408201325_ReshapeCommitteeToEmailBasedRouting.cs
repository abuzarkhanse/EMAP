using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReshapeCommitteeToEmailBasedRouting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProposalDefenseSchedules_FypCommittees_FypCommitteeId",
                table: "ProposalDefenseSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentGroups_FypCommittees_FypCommitteeId",
                table: "StudentGroups");

            migrationBuilder.DropIndex(
                name: "IX_StudentGroups_FypCommitteeId",
                table: "StudentGroups");

            migrationBuilder.DropIndex(
                name: "IX_ProposalDefenseSchedules_FypCommitteeId",
                table: "ProposalDefenseSchedules");

            migrationBuilder.DropColumn(
                name: "FypCommitteeId",
                table: "StudentGroups");

            migrationBuilder.DropColumn(
                name: "BulkBatchKey",
                table: "ProposalDefenseSchedules");

            migrationBuilder.DropColumn(
                name: "FypCommitteeId",
                table: "ProposalDefenseSchedules");

            migrationBuilder.DropColumn(
                name: "IsBulkAssigned",
                table: "ProposalDefenseSchedules");

            migrationBuilder.DropColumn(
                name: "ConvenorUserId",
                table: "FypCommittees");

            migrationBuilder.AddColumn<string>(
                name: "ConvenorEmail",
                table: "FypCommittees",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConvenorEmail",
                table: "FypCommittees");

            migrationBuilder.AddColumn<int>(
                name: "FypCommitteeId",
                table: "StudentGroups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BulkBatchKey",
                table: "ProposalDefenseSchedules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FypCommitteeId",
                table: "ProposalDefenseSchedules",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBulkAssigned",
                table: "ProposalDefenseSchedules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ConvenorUserId",
                table: "FypCommittees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentGroups_FypCommitteeId",
                table: "StudentGroups",
                column: "FypCommitteeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalDefenseSchedules_FypCommitteeId",
                table: "ProposalDefenseSchedules",
                column: "FypCommitteeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProposalDefenseSchedules_FypCommittees_FypCommitteeId",
                table: "ProposalDefenseSchedules",
                column: "FypCommitteeId",
                principalTable: "FypCommittees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGroups_FypCommittees_FypCommitteeId",
                table: "StudentGroups",
                column: "FypCommitteeId",
                principalTable: "FypCommittees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
