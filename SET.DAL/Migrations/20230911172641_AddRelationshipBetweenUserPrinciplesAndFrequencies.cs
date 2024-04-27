using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations;

/// <inheritdoc />
public partial class AddRelationshipBetweenUserPrinciplesAndFrequencies : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "FrequencyId",
            schema: "prc",
            table: "UserPrinciples",
            type: "uniqueidentifier",
            nullable: false);

        migrationBuilder.CreateIndex(
            name: "IX_UserPrinciples_FrequencyId",
            schema: "prc",
            table: "UserPrinciples",
            column: "FrequencyId");

        migrationBuilder.AddForeignKey(
            name: "FK_UserPrinciples_Frequencies_FrequencyId",
            schema: "prc",
            table: "UserPrinciples",
            column: "FrequencyId",
            principalSchema: "ntc",
            principalTable: "Frequencies",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_UserPrinciples_Frequencies_FrequencyId",
            schema: "prc",
            table: "UserPrinciples");

        migrationBuilder.DropIndex(
            name: "IX_UserPrinciples_FrequencyId",
            schema: "prc",
            table: "UserPrinciples");

        migrationBuilder.DropColumn(
            name: "FrequencyId",
            schema: "prc",
            table: "UserPrinciples");
    }
}
