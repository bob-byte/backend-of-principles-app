using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedReminderTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "sq__user_habit_reminders",
                schema: "hbt",
                startValue: 100L);

            migrationBuilder.CreateSequence(
                name: "sq__user_reminders",
                schema: "app",
                startValue: 100L);

            migrationBuilder.CreateSequence(
                name: "sq__week_days",
                schema: "app",
                startValue: 100L);

            migrationBuilder.CreateTable(
                name: "UserHabitReminders",
                schema: "hbt",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('hbt.sq__user_habit_reminders')"),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    UserHabitId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHabitReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserHabitReminders_UserHabits_UserHabitId",
                        column: x => x.UserHabitId,
                        principalSchema: "hbt",
                        principalTable: "UserHabits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserReminders",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('app.sq__user_reminders')"),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserReminders_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "app",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeekDays",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('app.sq__week_days')"),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeekDays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserHabitReminderWeekDay",
                columns: table => new
                {
                    DaysOfWeekId = table.Column<long>(type: "bigint", nullable: false),
                    RemindersId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHabitReminderWeekDay", x => new { x.DaysOfWeekId, x.RemindersId });
                    table.ForeignKey(
                        name: "FK_UserHabitReminderWeekDay_UserHabitReminders_RemindersId",
                        column: x => x.RemindersId,
                        principalSchema: "hbt",
                        principalTable: "UserHabitReminders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserHabitReminderWeekDay_WeekDays_DaysOfWeekId",
                        column: x => x.DaysOfWeekId,
                        principalSchema: "app",
                        principalTable: "WeekDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserHabitReminders_UserHabitId",
                schema: "hbt",
                table: "UserHabitReminders",
                column: "UserHabitId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHabitReminderWeekDay_RemindersId",
                table: "UserHabitReminderWeekDay",
                column: "RemindersId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReminders_UserId",
                schema: "app",
                table: "UserReminders",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserHabitReminderWeekDay");

            migrationBuilder.DropTable(
                name: "UserReminders",
                schema: "app");

            migrationBuilder.DropTable(
                name: "UserHabitReminders",
                schema: "hbt");

            migrationBuilder.DropTable(
                name: "WeekDays",
                schema: "app");

            migrationBuilder.DropSequence(
                name: "sq__user_habit_reminders",
                schema: "hbt");

            migrationBuilder.DropSequence(
                name: "sq__user_reminders",
                schema: "app");

            migrationBuilder.DropSequence(
                name: "sq__week_days",
                schema: "app");
        }
    }
}
