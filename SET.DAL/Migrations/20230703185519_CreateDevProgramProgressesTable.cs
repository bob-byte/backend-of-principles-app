using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations;

/// <inheritdoc />
public partial class CreateDevProgramProgressesTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DevProgramsProgresses",
            schema: "dev",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ChangedInPercent = table.Column<decimal>(type: "decimal(11,8)", nullable: false),
                TotalPrinciplesProgress = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                CountOfAchievedGoals = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DevProgramsProgresses", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DevProgramsProgresses",
            schema: "dev");
    }
}
