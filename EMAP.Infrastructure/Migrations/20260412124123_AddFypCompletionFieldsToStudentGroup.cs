using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFypCompletionFieldsToStudentGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "StudentGroups",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletionRemarks",
                table: "StudentGroups",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFypCompleted",
                table: "StudentGroups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStatusUpdatedAt",
                table: "StudentGroups",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReadyForLmsSync",
                table: "StudentGroups",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "StudentGroups");

            migrationBuilder.DropColumn(
                name: "CompletionRemarks",
                table: "StudentGroups");

            migrationBuilder.DropColumn(
                name: "IsFypCompleted",
                table: "StudentGroups");

            migrationBuilder.DropColumn(
                name: "LastStatusUpdatedAt",
                table: "StudentGroups");

            migrationBuilder.DropColumn(
                name: "ReadyForLmsSync",
                table: "StudentGroups");
        }
    }
}
