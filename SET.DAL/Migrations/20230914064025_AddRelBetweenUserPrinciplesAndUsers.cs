using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations;

/// <inheritdoc />
public partial class AddRelBetweenUserPrinciplesAndUsers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "UserId",
            schema: "prc",
            table: "UserPrinciples",
            type: "uniqueidentifier",
            nullable: false);

        migrationBuilder.CreateIndex(
            name: "IX_UserPrinciples_UserId",
            schema: "prc",
            table: "UserPrinciples",
            column: "UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_UserPrinciples_Users_UserId",
            schema: "prc",
            table: "UserPrinciples",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_UserPrinciples_Users_UserId",
            schema: "prc",
            table: "UserPrinciples");

        migrationBuilder.DropIndex(
            name: "IX_UserPrinciples_UserId",
            schema: "prc",
            table: "UserPrinciples");

        migrationBuilder.DropColumn(
            name: "UserId",
            schema: "prc",
            table: "UserPrinciples");
    }
}
