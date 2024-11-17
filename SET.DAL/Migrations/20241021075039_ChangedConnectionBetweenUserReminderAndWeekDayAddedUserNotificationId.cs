using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangedConnectionBetweenUserReminderAndWeekDayAddedUserNotificationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserHabitReminderWeekDay");

            migrationBuilder.AddColumn<long>(
                name: "UserHabitReminderId",
                schema: "app",
                table: "WeekDays",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserNotificationRequestId",
                schema: "app",
                table: "WeekDays",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserNotificationRequestId",
                schema: "app",
                table: "UserReminders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_WeekDays_UserHabitReminderId",
                schema: "app",
                table: "WeekDays",
                column: "UserHabitReminderId");

            migrationBuilder.AddForeignKey(
                name: "FK_WeekDays_UserHabitReminders_UserHabitReminderId",
                schema: "app",
                table: "WeekDays",
                column: "UserHabitReminderId",
                principalSchema: "hbt",
                principalTable: "UserHabitReminders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WeekDays_UserHabitReminders_UserHabitReminderId",
                schema: "app",
                table: "WeekDays");

            migrationBuilder.DropIndex(
                name: "IX_WeekDays_UserHabitReminderId",
                schema: "app",
                table: "WeekDays");

            migrationBuilder.DropColumn(
                name: "UserHabitReminderId",
                schema: "app",
                table: "WeekDays");

            migrationBuilder.DropColumn(
                name: "UserNotificationRequestId",
                schema: "app",
                table: "WeekDays");

            migrationBuilder.DropColumn(
                name: "UserNotificationRequestId",
                schema: "app",
                table: "UserReminders");

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
                name: "IX_UserHabitReminderWeekDay_RemindersId",
                table: "UserHabitReminderWeekDay",
                column: "RemindersId");
        }
    }
}
