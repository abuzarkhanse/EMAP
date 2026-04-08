using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFypCommitteeAndBulkProposalSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FypCommitteeId",
                table: "StudentGroups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgramCode",
                table: "StudentGroups",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.CreateTable(
                name: "FypCommittees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Session = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CoordinatorUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConvenorUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FypCommittees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FypCommitteePrograms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FypCommitteeId = table.Column<int>(type: "int", nullable: false),
                    ProgramCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FypCommitteePrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FypCommitteePrograms_FypCommittees_FypCommitteeId",
                        column: x => x.FypCommitteeId,
                        principalTable: "FypCommittees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentGroups_FypCommitteeId",
                table: "StudentGroups",
                column: "FypCommitteeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalDefenseSchedules_FypCommitteeId",
                table: "ProposalDefenseSchedules",
                column: "FypCommitteeId");

            migrationBuilder.CreateIndex(
                name: "IX_FypCommitteePrograms_FypCommitteeId_ProgramCode",
                table: "FypCommitteePrograms",
                columns: new[] { "FypCommitteeId", "ProgramCode" },
                unique: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProposalDefenseSchedules_FypCommittees_FypCommitteeId",
                table: "ProposalDefenseSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentGroups_FypCommittees_FypCommitteeId",
                table: "StudentGroups");

            migrationBuilder.DropTable(
                name: "FypCommitteePrograms");

            migrationBuilder.DropTable(
                name: "FypCommittees");

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
                name: "ProgramCode",
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
        }
    }
}
