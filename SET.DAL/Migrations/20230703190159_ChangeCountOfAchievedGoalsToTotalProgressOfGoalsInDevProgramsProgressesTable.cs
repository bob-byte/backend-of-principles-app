using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations;

/// <inheritdoc />
public partial class ChangeCountOfAchievedGoalsToTotalProgressOfGoalsInDevProgramsProgressesTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CountOfAchievedGoals",
            schema: "dev",
            table: "DevProgramsProgresses");

        migrationBuilder.AddColumn<decimal>(
            name: "TotalProgressOfGoals",
            schema: "dev",
            table: "DevProgramsProgresses",
            type: "decimal(18,8)",
            nullable: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TotalProgressOfGoals",
            schema: "dev",
            table: "DevProgramsProgresses");

        migrationBuilder.AddColumn<int>(
            name: "CountOfAchievedGoals",
            schema: "dev",
            table: "DevProgramsProgresses",
            type: "int",
            nullable: false);
    }
}
