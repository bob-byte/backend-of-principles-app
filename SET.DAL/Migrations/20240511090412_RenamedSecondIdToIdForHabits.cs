using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenamedSecondIdToIdForHabits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgressesOfHabits_UserHabits_HabitSecondIdEntityWithId",
                schema: "hbt",
                table: "ProgressesOfHabits");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAreasOfLifeUserHabits_UserHabits_SecondHabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropForeignKey(
                name: "FK_UserHabits_Frequencies_FrequencySecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropForeignKey(
                name: "FK_UserHabitUserHabit_UserHabits_ParentHabitsSecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabitUserHabit");

            migrationBuilder.DropForeignKey(
                name: "FK_UserHabitUserHabit_UserHabits_SubHabitsSecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabitUserHabit");

            migrationBuilder.DropIndex(
                name: "IX_UserHabits_FrequencySecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropIndex(
                name: "IX_ProgressesOfHabits_HabitSecondIdEntityWithId",
                schema: "hbt",
                table: "ProgressesOfHabits");

            migrationBuilder.DropColumn(
                name: "FrequencySecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropColumn(
                name: "HabitSecondIdEntityWithId",
                schema: "hbt",
                table: "ProgressesOfHabits");

            migrationBuilder.RenameColumn(
                name: "SubHabitsSecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabitUserHabit",
                newName: "SubHabitsId");

            migrationBuilder.RenameColumn(
                name: "ParentHabitsSecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabitUserHabit",
                newName: "ParentHabitsId");

            migrationBuilder.RenameIndex(
                name: "IX_UserHabitUserHabit_SubHabitsSecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabitUserHabit",
                newName: "IX_UserHabitUserHabit_SubHabitsId");

            migrationBuilder.RenameColumn(
                name: "SecondFrequencyId",
                schema: "hbt",
                table: "UserHabits",
                newName: "FrequencyId");

            migrationBuilder.RenameColumn(
                name: "SecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabits",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "SecondHabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                newName: "HabitId");

            migrationBuilder.RenameIndex(
                name: "IX_UserAreasOfLifeUserHabits_SecondHabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                newName: "IX_UserAreasOfLifeUserHabits_HabitId");

            migrationBuilder.RenameColumn(
                name: "SecondHabitId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                newName: "HabitId");

            migrationBuilder.RenameColumn(
                name: "SecondIdEntityWithId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "SecondIdEntityWithId",
                schema: "ntc",
                table: "Frequencies",
                newName: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserHabits_FrequencyId",
                schema: "hbt",
                table: "UserHabits",
                column: "FrequencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressesOfHabits_HabitId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                column: "HabitId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressesOfHabits_UserHabits_HabitId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                column: "HabitId",
                principalSchema: "hbt",
                principalTable: "UserHabits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAreasOfLifeUserHabits_UserHabits_HabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                column: "HabitId",
                principalSchema: "hbt",
                principalTable: "UserHabits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserHabits_Frequencies_FrequencyId",
                schema: "hbt",
                table: "UserHabits",
                column: "FrequencyId",
                principalSchema: "ntc",
                principalTable: "Frequencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserHabitUserHabit_UserHabits_ParentHabitsId",
                schema: "hbt",
                table: "UserHabitUserHabit",
                column: "ParentHabitsId",
                principalSchema: "hbt",
                principalTable: "UserHabits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserHabitUserHabit_UserHabits_SubHabitsId",
                schema: "hbt",
                table: "UserHabitUserHabit",
                column: "SubHabitsId",
                principalSchema: "hbt",
                principalTable: "UserHabits",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgressesOfHabits_UserHabits_HabitId",
                schema: "hbt",
                table: "ProgressesOfHabits");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAreasOfLifeUserHabits_UserHabits_HabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropForeignKey(
                name: "FK_UserHabits_Frequencies_FrequencyId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropForeignKey(
                name: "FK_UserHabitUserHabit_UserHabits_ParentHabitsId",
                schema: "hbt",
                table: "UserHabitUserHabit");

            migrationBuilder.DropForeignKey(
                name: "FK_UserHabitUserHabit_UserHabits_SubHabitsId",
                schema: "hbt",
                table: "UserHabitUserHabit");

            migrationBuilder.DropIndex(
                name: "IX_UserHabits_FrequencyId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropIndex(
                name: "IX_ProgressesOfHabits_HabitId",
                schema: "hbt",
                table: "ProgressesOfHabits");

            migrationBuilder.RenameColumn(
                name: "SubHabitsId",
                schema: "hbt",
                table: "UserHabitUserHabit",
                newName: "SubHabitsSecondIdEntityWithId");

            migrationBuilder.RenameColumn(
                name: "ParentHabitsId",
                schema: "hbt",
                table: "UserHabitUserHabit",
                newName: "ParentHabitsSecondIdEntityWithId");

            migrationBuilder.RenameIndex(
                name: "IX_UserHabitUserHabit_SubHabitsId",
                schema: "hbt",
                table: "UserHabitUserHabit",
                newName: "IX_UserHabitUserHabit_SubHabitsSecondIdEntityWithId");

            migrationBuilder.RenameColumn(
                name: "FrequencyId",
                schema: "hbt",
                table: "UserHabits",
                newName: "SecondFrequencyId");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "hbt",
                table: "UserHabits",
                newName: "SecondIdEntityWithId");

            migrationBuilder.RenameColumn(
                name: "HabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                newName: "SecondHabitId");

            migrationBuilder.RenameIndex(
                name: "IX_UserAreasOfLifeUserHabits_HabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                newName: "IX_UserAreasOfLifeUserHabits_SecondHabitId");

            migrationBuilder.RenameColumn(
                name: "HabitId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                newName: "SecondHabitId");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "hbt",
                table: "ProgressesOfHabits",
                newName: "SecondIdEntityWithId");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "ntc",
                table: "Frequencies",
                newName: "SecondIdEntityWithId");

            migrationBuilder.AddColumn<long>(
                name: "FrequencySecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabits",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "HabitSecondIdEntityWithId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_UserHabits_FrequencySecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabits",
                column: "FrequencySecondIdEntityWithId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressesOfHabits_HabitSecondIdEntityWithId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                column: "HabitSecondIdEntityWithId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressesOfHabits_UserHabits_HabitSecondIdEntityWithId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                column: "HabitSecondIdEntityWithId",
                principalSchema: "hbt",
                principalTable: "UserHabits",
                principalColumn: "SecondIdEntityWithId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAreasOfLifeUserHabits_UserHabits_SecondHabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                column: "SecondHabitId",
                principalSchema: "hbt",
                principalTable: "UserHabits",
                principalColumn: "SecondIdEntityWithId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserHabits_Frequencies_FrequencySecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabits",
                column: "FrequencySecondIdEntityWithId",
                principalSchema: "ntc",
                principalTable: "Frequencies",
                principalColumn: "SecondIdEntityWithId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserHabitUserHabit_UserHabits_ParentHabitsSecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabitUserHabit",
                column: "ParentHabitsSecondIdEntityWithId",
                principalSchema: "hbt",
                principalTable: "UserHabits",
                principalColumn: "SecondIdEntityWithId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserHabitUserHabit_UserHabits_SubHabitsSecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabitUserHabit",
                column: "SubHabitsSecondIdEntityWithId",
                principalSchema: "hbt",
                principalTable: "UserHabits",
                principalColumn: "SecondIdEntityWithId");
        }
    }
}
