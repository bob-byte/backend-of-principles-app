using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SET.DataAccess.Migrations;

/// <inheritdoc />
public partial class DeleteWeightColumnAndAddGenderToUserTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Weight",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "ChallangeName",
            table: "Challenges");

        migrationBuilder.AddColumn<int>(
            name: "Gender",
            table: "Users",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "Name",
            table: "Challenges",
            type: "nvarchar(255)",
            nullable: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Gender",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "Name",
            table: "Challenges");

        migrationBuilder.AddColumn<double>(
            name: "Weight",
            table: "Users",
            type: "float",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "ChallangeName",
            table: "Challenges",
            type: "int",
            nullable: false,
            defaultValue: 0);
    }
}
