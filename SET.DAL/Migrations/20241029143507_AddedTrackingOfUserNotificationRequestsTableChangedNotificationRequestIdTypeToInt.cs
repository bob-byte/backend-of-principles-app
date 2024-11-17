using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedTrackingOfUserNotificationRequestsTableChangedNotificationRequestIdTypeToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "sq__tracking__of__user__notification__requests",
                schema: "app",
                startValue: 100L);

            migrationBuilder.AlterColumn<int>(
                name: "UserNotificationRequestId",
                schema: "app",
                table: "WeekDays",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "UserNotificationRequestId",
                schema: "app",
                table: "UserReminders",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateTable(
                name: "TrackingOfUserNotificationRequests",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('app.sq__tracking__of__user__notification__requests')"),
                    MaxNotificationRequestId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackingOfUserNotificationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackingOfUserNotificationRequests_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "app",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackingOfUserNotificationRequests_UserId",
                schema: "app",
                table: "TrackingOfUserNotificationRequests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackingOfUserNotificationRequests",
                schema: "app");

            migrationBuilder.DropSequence(
                name: "sq__tracking__of__user__notification__requests",
                schema: "app");

            migrationBuilder.AlterColumn<long>(
                name: "UserNotificationRequestId",
                schema: "app",
                table: "WeekDays",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "UserNotificationRequestId",
                schema: "app",
                table: "UserReminders",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
