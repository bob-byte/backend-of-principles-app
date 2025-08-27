using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedHabitKindToUserHabit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Kind",
                schema: "hbt",
                table: "UserHabits",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "MaxRate",
                schema: "hbt",
                table: "UserHabits",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MinRate",
                schema: "hbt",
                table: "UserHabits",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TargetPerOneTime",
                schema: "hbt",
                table: "UserHabits",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetType",
                schema: "hbt",
                table: "UserHabits",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                schema: "hbt",
                table: "UserHabits",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kind",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropColumn(
                name: "MaxRate",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropColumn(
                name: "MinRate",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropColumn(
                name: "TargetPerOneTime",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropColumn(
                name: "TargetType",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropColumn(
                name: "Unit",
                schema: "hbt",
                table: "UserHabits");
        }
    }
}
