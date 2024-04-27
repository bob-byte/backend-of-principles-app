using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations;

/// <inheritdoc />
public partial class DeletePersonalPrinciplesTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PersonalPrincipleUser");

        migrationBuilder.DropTable(
            name: "PersonalPrinciples");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PersonalPrinciples",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FullDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ReasonToFollow = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PersonalPrinciples", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "PersonalPrincipleUser",
            columns: table => new
            {
                PersonalPrinciplesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UsersId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PersonalPrincipleUser", x => new { x.PersonalPrinciplesId, x.UsersId });
                table.ForeignKey(
                    name: "FK_PersonalPrincipleUser_PersonalPrinciples_PersonalPrinciplesId",
                    column: x => x.PersonalPrinciplesId,
                    principalTable: "PersonalPrinciples",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PersonalPrincipleUser_Users_UsersId",
                    column: x => x.UsersId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PersonalPrincipleUser_UsersId",
            table: "PersonalPrincipleUser",
            column: "UsersId");
    }
}
