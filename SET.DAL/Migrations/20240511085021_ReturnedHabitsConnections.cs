using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ReturnedHabitsConnections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAreasOfLifeUserHabits_UserHabits_UserHabitSecondIdEntityWithId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropIndex(
                name: "IX_UserAreasOfLifeUserHabits_HabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropIndex(
                name: "IX_UserAreasOfLifeUserHabits_UserHabitSecondIdEntityWithId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropColumn(
                name: "FrequencyId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropColumn(
                name: "HabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropColumn(
                name: "UserHabitSecondIdEntityWithId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropColumn(
                name: "HabitId",
                schema: "hbt",
                table: "ProgressesOfHabits");

            migrationBuilder.CreateTable(
                name: "UserHabitUserHabit",
                schema: "hbt",
                columns: table => new
                {
                    ParentHabitsSecondIdEntityWithId = table.Column<long>(type: "bigint", nullable: false),
                    SubHabitsSecondIdEntityWithId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHabitUserHabit", x => new { x.ParentHabitsSecondIdEntityWithId, x.SubHabitsSecondIdEntityWithId });
                    table.ForeignKey(
                        name: "FK_UserHabitUserHabit_UserHabits_ParentHabitsSecondIdEntityWithId",
                        column: x => x.ParentHabitsSecondIdEntityWithId,
                        principalSchema: "hbt",
                        principalTable: "UserHabits",
                        principalColumn: "SecondIdEntityWithId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserHabitUserHabit_UserHabits_SubHabitsSecondIdEntityWithId",
                        column: x => x.SubHabitsSecondIdEntityWithId,
                        principalSchema: "hbt",
                        principalTable: "UserHabits",
                        principalColumn: "SecondIdEntityWithId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAreasOfLifeUserHabits_SecondHabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                column: "SecondHabitId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHabitUserHabit_SubHabitsSecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabitUserHabit",
                column: "SubHabitsSecondIdEntityWithId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAreasOfLifeUserHabits_UserHabits_SecondHabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                column: "SecondHabitId",
                principalSchema: "hbt",
                principalTable: "UserHabits",
                principalColumn: "SecondIdEntityWithId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAreasOfLifeUserHabits_UserHabits_SecondHabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropTable(
                name: "UserHabitUserHabit",
                schema: "hbt");

            migrationBuilder.DropIndex(
                name: "IX_UserAreasOfLifeUserHabits_SecondHabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.AddColumn<Guid>(
                name: "FrequencyId",
                schema: "hbt",
                table: "UserHabits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "HabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UserHabitSecondIdEntityWithId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HabitId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_UserAreasOfLifeUserHabits_HabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                column: "HabitId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAreasOfLifeUserHabits_UserHabitSecondIdEntityWithId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                column: "UserHabitSecondIdEntityWithId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAreasOfLifeUserHabits_UserHabits_UserHabitSecondIdEntityWithId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                column: "UserHabitSecondIdEntityWithId",
                principalSchema: "hbt",
                principalTable: "UserHabits",
                principalColumn: "SecondIdEntityWithId");
        }
    }
}
