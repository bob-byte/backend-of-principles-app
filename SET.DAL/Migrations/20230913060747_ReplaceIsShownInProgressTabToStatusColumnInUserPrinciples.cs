using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations;

/// <inheritdoc />
public partial class ReplaceIsShownInProgressTabToStatusColumnInUserPrinciples : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsShownInProgressTab",
            schema: "prc",
            table: "UserPrinciples");

        migrationBuilder.AddColumn<int>(
            name: "Status",
            schema: "prc",
            table: "UserPrinciples",
            type: "int",
            nullable: false,
            defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Status",
            schema: "prc",
            table: "UserPrinciples");

        migrationBuilder.AddColumn<bool>(
            name: "IsShownInProgressTab",
            schema: "prc",
            table: "UserPrinciples",
            type: "bit",
            nullable: false,
            defaultValue: false);
    }
}
