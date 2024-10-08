using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTypeFromDateTimeToTimeOnlyForReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeOnly>(
                name: "Time",
                schema: "app",
                table: "UserReminders",
                type: "time without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "Time",
                schema: "hbt",
                table: "UserHabitReminders",
                type: "time without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Time",
                schema: "app",
                table: "UserReminders",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Time",
                schema: "hbt",
                table: "UserHabitReminders",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");
        }
    }
}
