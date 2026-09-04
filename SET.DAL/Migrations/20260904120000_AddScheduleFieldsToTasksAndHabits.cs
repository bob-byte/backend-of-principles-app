using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleFieldsToTasksAndHabits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                schema: "tsk",
                table: "Tasks",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                schema: "tsk",
                table: "Tasks",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllDay",
                schema: "tsk",
                table: "Tasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ConstantReminder",
                schema: "tsk",
                table: "Tasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ConstantNotificationRequestId",
                schema: "tsk",
                table: "Tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemindersJson",
                schema: "tsk",
                table: "Tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepeatJson",
                schema: "tsk",
                table: "Tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                schema: "hbt",
                table: "UserHabits",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                schema: "hbt",
                table: "UserHabits",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllDay",
                schema: "hbt",
                table: "UserHabits",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ConstantReminder",
                schema: "hbt",
                table: "UserHabits",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OffsetsJson",
                schema: "hbt",
                table: "UserHabitReminders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ConstantReminder",
                schema: "hbt",
                table: "UserHabitReminders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ConstantNotificationRequestId",
                schema: "hbt",
                table: "UserHabitReminders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                schema: "hbt",
                table: "UserHabitReminders",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllDay",
                schema: "hbt",
                table: "UserHabitReminders",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "EndDate", schema: "tsk", table: "Tasks");
            migrationBuilder.DropColumn(name: "EndTime", schema: "tsk", table: "Tasks");
            migrationBuilder.DropColumn(name: "AllDay", schema: "tsk", table: "Tasks");
            migrationBuilder.DropColumn(name: "ConstantReminder", schema: "tsk", table: "Tasks");
            migrationBuilder.DropColumn(name: "ConstantNotificationRequestId", schema: "tsk", table: "Tasks");
            migrationBuilder.DropColumn(name: "RemindersJson", schema: "tsk", table: "Tasks");
            migrationBuilder.DropColumn(name: "RepeatJson", schema: "tsk", table: "Tasks");

            migrationBuilder.DropColumn(name: "EndDate", schema: "hbt", table: "UserHabits");
            migrationBuilder.DropColumn(name: "EndTime", schema: "hbt", table: "UserHabits");
            migrationBuilder.DropColumn(name: "AllDay", schema: "hbt", table: "UserHabits");
            migrationBuilder.DropColumn(name: "ConstantReminder", schema: "hbt", table: "UserHabits");

            migrationBuilder.DropColumn(name: "OffsetsJson", schema: "hbt", table: "UserHabitReminders");
            migrationBuilder.DropColumn(name: "ConstantReminder", schema: "hbt", table: "UserHabitReminders");
            migrationBuilder.DropColumn(name: "ConstantNotificationRequestId", schema: "hbt", table: "UserHabitReminders");
            migrationBuilder.DropColumn(name: "EndTime", schema: "hbt", table: "UserHabitReminders");
            migrationBuilder.DropColumn(name: "AllDay", schema: "hbt", table: "UserHabitReminders");
        }
    }
}
