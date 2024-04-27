using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations;

/// <inheritdoc />
public partial class AddColTypeOfDevPrograms : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "TypeId",
            schema: "dev",
            table: "ComplicatedDevPrograms",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "TypeOfComplicatedDevProgram",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<int>(type: "int", nullable: false),
                IsDisabled = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TypeOfComplicatedDevProgram", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ComplicatedDevPrograms_TypeId",
            schema: "dev",
            table: "ComplicatedDevPrograms",
            column: "TypeId");

        migrationBuilder.AddForeignKey(
            name: "FK_ComplicatedDevPrograms_TypeOfComplicatedDevProgram_TypeId",
            schema: "dev",
            table: "ComplicatedDevPrograms",
            column: "TypeId",
            principalTable: "TypeOfComplicatedDevProgram",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ComplicatedDevPrograms_TypeOfComplicatedDevProgram_TypeId",
            schema: "dev",
            table: "ComplicatedDevPrograms");

        migrationBuilder.DropTable(
            name: "TypeOfComplicatedDevProgram");

        migrationBuilder.DropIndex(
            name: "IX_ComplicatedDevPrograms_TypeId",
            schema: "dev",
            table: "ComplicatedDevPrograms");

        migrationBuilder.DropColumn(
            name: "TypeId",
            schema: "dev",
            table: "ComplicatedDevPrograms");
    }
}
