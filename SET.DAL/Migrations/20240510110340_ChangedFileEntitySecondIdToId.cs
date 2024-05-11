using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangedFileEntitySecondIdToId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FilesEntities_Users_UserId",
                schema: "flt",
                table: "FilesEntities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FilesEntities",
                schema: "flt",
                table: "FilesEntities");

            migrationBuilder.RenameTable(
                name: "FilesEntities",
                schema: "flt",
                newName: "FileEntities",
                newSchema: "flt");

            migrationBuilder.RenameColumn(
                name: "SecondId",
                schema: "flt",
                table: "FileEntities",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_FilesEntities_UserId",
                schema: "flt",
                table: "FileEntities",
                newName: "IX_FileEntities_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FileEntities",
                schema: "flt",
                table: "FileEntities",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FileEntities_Users_UserId",
                schema: "flt",
                table: "FileEntities",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileEntities_Users_UserId",
                schema: "flt",
                table: "FileEntities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FileEntities",
                schema: "flt",
                table: "FileEntities");

            migrationBuilder.RenameTable(
                name: "FileEntities",
                schema: "flt",
                newName: "FilesEntities",
                newSchema: "flt");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "flt",
                table: "FilesEntities",
                newName: "SecondId");

            migrationBuilder.RenameIndex(
                name: "IX_FileEntities_UserId",
                schema: "flt",
                table: "FilesEntities",
                newName: "IX_FilesEntities_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FilesEntities",
                schema: "flt",
                table: "FilesEntities",
                column: "SecondId");

            migrationBuilder.AddForeignKey(
                name: "FK_FilesEntities_Users_UserId",
                schema: "flt",
                table: "FilesEntities",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
