using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class DeleteGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NoticeReminder");

            migrationBuilder.DropTable(
                name: "TimeZone");

            migrationBuilder.DropTable(
                name: "Reminder");

            migrationBuilder.DropTable(
                name: "Notice");

            migrationBuilder.DropTable(
                name: "Goals");

            migrationBuilder.DropTable(
                name: "NoticeRepeat");

            migrationBuilder.DropTable(
                name: "BuiltInFrequency");

            migrationBuilder.DropTable(
                name: "EndRepeat");

            migrationBuilder.DropTable(
                name: "UserFrequency");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuiltInFrequency",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuiltInFrequency", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EndRepeat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Never = table.Column<bool>(type: "bit", nullable: true),
                    Timer = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndRepeat", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Goals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityArea = table.Column<int>(type: "int", nullable: false),
                    CompletionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReasonToAchieve = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Goals_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reminder",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimeToMainNotice = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminder", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserFrequency",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Frequency = table.Column<TimeOnly>(type: "time", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFrequency", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NoticeRepeat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuiltInFrequencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EndRepeatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserFrequencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoticeRepeat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoticeRepeat_BuiltInFrequency_BuiltInFrequencyId",
                        column: x => x.BuiltInFrequencyId,
                        principalTable: "BuiltInFrequency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NoticeRepeat_EndRepeat_EndRepeatId",
                        column: x => x.EndRepeatId,
                        principalTable: "EndRepeat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NoticeRepeat_UserFrequency_UserFrequencyId",
                        column: x => x.UserFrequencyId,
                        principalTable: "UserFrequency",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RepeatId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    From = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAllDay = table.Column<bool>(type: "bit", nullable: false),
                    To = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notice_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notice_NoticeRepeat_RepeatId",
                        column: x => x.RepeatId,
                        principalTable: "NoticeRepeat",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NoticeReminder",
                columns: table => new
                {
                    NoticesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RemindersId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoticeReminder", x => new { x.NoticesId, x.RemindersId });
                    table.ForeignKey(
                        name: "FK_NoticeReminder_Notice_NoticesId",
                        column: x => x.NoticesId,
                        principalTable: "Notice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NoticeReminder_Reminder_RemindersId",
                        column: x => x.RemindersId,
                        principalTable: "Reminder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeZone",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoticeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Zone = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeZone", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeZone_Notice_NoticeId",
                        column: x => x.NoticeId,
                        principalTable: "Notice",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Goals_UserId",
                table: "Goals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notice_GoalId",
                table: "Notice",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_Notice_RepeatId",
                table: "Notice",
                column: "RepeatId");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeReminder_RemindersId",
                table: "NoticeReminder",
                column: "RemindersId");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeRepeat_BuiltInFrequencyId",
                table: "NoticeRepeat",
                column: "BuiltInFrequencyId");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeRepeat_EndRepeatId",
                table: "NoticeRepeat",
                column: "EndRepeatId");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeRepeat_UserFrequencyId",
                table: "NoticeRepeat",
                column: "UserFrequencyId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeZone_NoticeId",
                table: "TimeZone",
                column: "NoticeId",
                unique: true,
                filter: "[NoticeId] IS NOT NULL");
        }
    }
}
