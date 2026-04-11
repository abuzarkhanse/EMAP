using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStageToFypChapterAnnouncementAndFyp2ChapterTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Stage",
                table: "FypChapterAnnouncements",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stage",
                table: "FypChapterAnnouncements");
        }
    }
}
