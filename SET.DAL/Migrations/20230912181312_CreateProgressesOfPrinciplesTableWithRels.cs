using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations;

/// <inheritdoc />
public partial class CreateProgressesOfPrinciplesTableWithRels : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ProgressesOfPrinciples",
            schema: "prc",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                Date = table.Column<DateOnly>(type: "date", nullable: false),
                Value = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                PrincipleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProgressesOfPrinciples", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProgressesOfPrinciples_UserPrinciples_PrincipleId",
                    column: x => x.PrincipleId,
                    principalSchema: "prc",
                    principalTable: "UserPrinciples",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProgressesOfPrinciples_PrincipleId",
            schema: "prc",
            table: "ProgressesOfPrinciples",
            column: "PrincipleId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ProgressesOfPrinciples",
            schema: "prc");
    }
}
