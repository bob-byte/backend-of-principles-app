using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedSecondIdForHabits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SecondFrequencyId",
                schema: "hbt",
                table: "UserHabits",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabits",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SecondHabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SecondHabitId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SecondIdEntityWithId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SecondIdEntityWithId",
                schema: "ntc",
                table: "Frequencies",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecondFrequencyId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropColumn(
                name: "SecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropColumn(
                name: "SecondHabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropColumn(
                name: "SecondHabitId",
                schema: "hbt",
                table: "ProgressesOfHabits");

            migrationBuilder.DropColumn(
                name: "SecondIdEntityWithId",
                schema: "hbt",
                table: "ProgressesOfHabits");

            migrationBuilder.DropColumn(
                name: "SecondIdEntityWithId",
                schema: "ntc",
                table: "Frequencies");
        }
    }
}
