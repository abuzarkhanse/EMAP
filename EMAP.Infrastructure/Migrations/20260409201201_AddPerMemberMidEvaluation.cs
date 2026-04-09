using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerMemberMidEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FypEvaluationMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluationId = table.Column<int>(type: "int", nullable: false),
                    StudentUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    StudentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RegistrationNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TotalMarks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WeightedMarks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FypEvaluationMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FypEvaluationMembers_FypEvaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "FypEvaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FypEvaluationMemberScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluationMemberId = table.Column<int>(type: "int", nullable: false),
                    CriterionId = table.Column<int>(type: "int", nullable: false),
                    AwardedMarks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FypEvaluationMemberScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FypEvaluationMemberScores_FypEvaluationCriteria_CriterionId",
                        column: x => x.CriterionId,
                        principalTable: "FypEvaluationCriteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FypEvaluationMemberScores_FypEvaluationMembers_EvaluationMemberId",
                        column: x => x.EvaluationMemberId,
                        principalTable: "FypEvaluationMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FypEvaluationMembers_EvaluationId",
                table: "FypEvaluationMembers",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_FypEvaluationMemberScores_CriterionId",
                table: "FypEvaluationMemberScores",
                column: "CriterionId");

            migrationBuilder.CreateIndex(
                name: "IX_FypEvaluationMemberScores_EvaluationMemberId",
                table: "FypEvaluationMemberScores",
                column: "EvaluationMemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FypEvaluationMemberScores");

            migrationBuilder.DropTable(
                name: "FypEvaluationMembers");
        }
    }
}
