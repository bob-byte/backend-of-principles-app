using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations;

/// <inheritdoc />
public partial class AddRelBetweenUsersAndUserAreasOfLife : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "Status",
            schema: "prc",
            table: "UserPrinciples",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AddColumn<Guid>(
            name: "UserId",
            schema: "arlf",
            table: "UserAreasOfLife",
            type: "uniqueidentifier",
            nullable: false);

        migrationBuilder.CreateIndex(
            name: "IX_UserAreasOfLife_UserId",
            schema: "arlf",
            table: "UserAreasOfLife",
            column: "UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_UserAreasOfLife_Users_UserId",
            schema: "arlf",
            table: "UserAreasOfLife",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_UserAreasOfLife_Users_UserId",
            schema: "arlf",
            table: "UserAreasOfLife");

        migrationBuilder.DropIndex(
            name: "IX_UserAreasOfLife_UserId",
            schema: "arlf",
            table: "UserAreasOfLife");

        migrationBuilder.DropColumn(
            name: "UserId",
            schema: "arlf",
            table: "UserAreasOfLife");

        migrationBuilder.AlterColumn<int>(
            name: "Status",
            schema: "prc",
            table: "UserPrinciples",
            type: "int",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int",
            oldDefaultValue: 0);
    }
}
