using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SET.DataAccess.Migrations;

public partial class AddRelationshipsOfChallengeModel : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Rd71s_Users_UserId",
            table: "Rd71s");

        migrationBuilder.DropIndex(
            name: "IX_Rd71s_UserId",
            table: "Rd71s");

        migrationBuilder.RenameColumn(
            name: "UserId",
            table: "Rd71s",
            newName: "ChallengeId");

        migrationBuilder.AddColumn<int>(
            name: "UserType",
            table: "Users",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "Foundator",
            table: "Challenges",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<Guid>(
            name: "UserId",
            table: "Challenges",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Rd71s_ChallengeId",
            table: "Rd71s",
            column: "ChallengeId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Challenges_UserId",
            table: "Challenges",
            column: "UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_Challenges_Users_UserId",
            table: "Challenges",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Rd71s_Challenges_ChallengeId",
            table: "Rd71s",
            column: "ChallengeId",
            principalTable: "Challenges",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Challenges_Users_UserId",
            table: "Challenges");

        migrationBuilder.DropForeignKey(
            name: "FK_Rd71s_Challenges_ChallengeId",
            table: "Rd71s");

        migrationBuilder.DropIndex(
            name: "IX_Rd71s_ChallengeId",
            table: "Rd71s");

        migrationBuilder.DropIndex(
            name: "IX_Challenges_UserId",
            table: "Challenges");

        migrationBuilder.DropColumn(
            name: "UserType",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "Foundator",
            table: "Challenges");

        migrationBuilder.DropColumn(
            name: "UserId",
            table: "Challenges");

        migrationBuilder.RenameColumn(
            name: "ChallengeId",
            table: "Rd71s",
            newName: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Rd71s_UserId",
            table: "Rd71s",
            column: "UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_Rd71s_Users_UserId",
            table: "Rd71s",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
