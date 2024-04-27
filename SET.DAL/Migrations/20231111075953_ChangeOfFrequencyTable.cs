using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeOfFrequencyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "ntc",
                table: "Frequencies",
                newName: "Type");

            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                schema: "ntc",
                table: "Frequencies",
                type: "decimal(7,6)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntervalLengthInDays",
                schema: "ntc",
                table: "Frequencies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Repeats",
                schema: "ntc",
                table: "Frequencies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntervalLengthInDays",
                schema: "ntc",
                table: "Frequencies");

            migrationBuilder.DropColumn(
                name: "Repeats",
                schema: "ntc",
                table: "Frequencies");

            migrationBuilder.RenameColumn(
                name: "Type",
                schema: "ntc",
                table: "Frequencies",
                newName: "Name");

            migrationBuilder.AlterColumn<int>(
                name: "Value",
                schema: "ntc",
                table: "Frequencies",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(7,6)");
        }
    }
}
