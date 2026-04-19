using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAtFieldsForFullSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "app",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "app",
                table: "UserReminders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "hbt",
                table: "ProgressesOfHabits",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "app",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "app",
                table: "UserReminders");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "hbt",
                table: "ProgressesOfHabits");
        }
    }
}
