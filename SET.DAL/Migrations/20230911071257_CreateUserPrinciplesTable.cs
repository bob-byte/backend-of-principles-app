using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations;

/// <inheritdoc />
public partial class CreateUserPrinciplesTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserPrinciples",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ReasonToFollow = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Question = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsShownInProgressTab = table.Column<bool>(type: "bit", nullable: false),
                PercentageAchieved = table.Column<decimal>(type: "decimal(8,5)", nullable: false),
                ColorName = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserPrinciples", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "UserPrincipleUserPrinciple",
            columns: table => new
            {
                ParentPrinciplesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SubPrinciplesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserPrincipleUserPrinciple", x => new { x.ParentPrinciplesId, x.SubPrinciplesId });
                table.ForeignKey(
                    name: "FK_UserPrincipleUserPrinciple_UserPrinciples_ParentPrinciplesId",
                    column: x => x.ParentPrinciplesId,
                    principalTable: "UserPrinciples",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserPrincipleUserPrinciple_UserPrinciples_SubPrinciplesId",
                    column: x => x.SubPrinciplesId,
                    principalTable: "UserPrinciples",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserPrincipleUserPrinciple_SubPrinciplesId",
            table: "UserPrincipleUserPrinciple",
            column: "SubPrinciplesId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserPrincipleUserPrinciple");

        migrationBuilder.DropTable(
            name: "UserPrinciples");
    }
}
