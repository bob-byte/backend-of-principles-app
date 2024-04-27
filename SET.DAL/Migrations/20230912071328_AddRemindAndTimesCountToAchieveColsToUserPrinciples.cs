using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations;

/// <inheritdoc />
public partial class AddRemindAndTimesCountToAchieveColsToUserPrinciples : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<TimeOnly>(
            name: "Remind",
            schema: "prc",
            table: "UserPrinciples",
            type: "time",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "TimesCountToAchieve",
            schema: "prc",
            table: "UserPrinciples",
            type: "int",
            nullable: false,
            defaultValue: 100);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Remind",
            schema: "prc",
            table: "UserPrinciples");

        migrationBuilder.DropColumn(
            name: "TimesCountToAchieve",
            schema: "prc",
            table: "UserPrinciples");
    }
}
