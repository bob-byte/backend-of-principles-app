using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations;

/// <inheritdoc />
public partial class AddRelationshipsBetweenUserAreaOfLifeAndUserPrinciple : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "prc");

        migrationBuilder.RenameTable(
            name: "UserPrincipleUserPrinciple",
            newName: "UserPrincipleUserPrinciple",
            newSchema: "prc");

        migrationBuilder.RenameTable(
            name: "UserPrinciples",
            newName: "UserPrinciples",
            newSchema: "prc");

        migrationBuilder.CreateTable(
            name: "UserAreasOfLifeUserPrinciples",
            schema: "arlf",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AreaOfLifeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PrincipleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PriorityOfPrinciple = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserAreasOfLifeUserPrinciples", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserAreasOfLifeUserPrinciples_UserAreasOfLife_AreaOfLifeId",
                    column: x => x.AreaOfLifeId,
                    principalSchema: "arlf",
                    principalTable: "UserAreasOfLife",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserAreasOfLifeUserPrinciples_UserPrinciples_PrincipleId",
                    column: x => x.PrincipleId,
                    principalSchema: "prc",
                    principalTable: "UserPrinciples",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserAreasOfLifeUserPrinciples_AreaOfLifeId",
            schema: "arlf",
            table: "UserAreasOfLifeUserPrinciples",
            column: "AreaOfLifeId");

        migrationBuilder.CreateIndex(
            name: "IX_UserAreasOfLifeUserPrinciples_PrincipleId",
            schema: "arlf",
            table: "UserAreasOfLifeUserPrinciples",
            column: "PrincipleId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserAreasOfLifeUserPrinciples",
            schema: "arlf");

        migrationBuilder.RenameTable(
            name: "UserPrincipleUserPrinciple",
            schema: "prc",
            newName: "UserPrincipleUserPrinciple");

        migrationBuilder.RenameTable(
            name: "UserPrinciples",
            schema: "prc",
            newName: "UserPrinciples");
    }
}
