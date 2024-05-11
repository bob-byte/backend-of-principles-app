using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedSecondUserIdForFileEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileEntities_Users_UserId1",
                schema: "flt",
                table: "FileEntities");

            migrationBuilder.DropIndex(
                name: "IX_FileEntities_UserId1",
                schema: "flt",
                table: "FileEntities");

            migrationBuilder.RenameColumn(
                name: "UserId1",
                schema: "flt",
                table: "FileEntities",
                newName: "SecondUserId");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                schema: "flt",
                table: "FileEntities",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SecondUserId",
                schema: "flt",
                table: "FileEntities",
                newName: "UserId1");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                schema: "flt",
                table: "FileEntities",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_FileEntities_UserId1",
                schema: "flt",
                table: "FileEntities",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_FileEntities_Users_UserId1",
                schema: "flt",
                table: "FileEntities",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
