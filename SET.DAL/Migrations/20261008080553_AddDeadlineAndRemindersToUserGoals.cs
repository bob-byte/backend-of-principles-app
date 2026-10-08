using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddDeadlineAndRemindersToUserGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "Deadline",
                schema: "goal",
                table: "UserGoals",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemindersJson",
                schema: "goal",
                table: "UserGoals",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deadline",
                schema: "goal",
                table: "UserGoals");

            migrationBuilder.DropColumn(
                name: "RemindersJson",
                schema: "goal",
                table: "UserGoals");
        }
    }
}
