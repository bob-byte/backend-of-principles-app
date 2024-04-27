using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPriorityColToUserHabitsAndChangeTypeOfValueInProgressesOfHabitsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "hbt",
                table: "UserHabits",
                type: "int",
                nullable: false,
                defaultValue: 0 );

            migrationBuilder.AlterColumn<int>(
                name: "Value",
                schema: "hbt",
                table: "ProgressesOfHabits",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,4)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                schema: "hbt",
                table: "ProgressesOfHabits",
                type: "decimal(10,4)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
