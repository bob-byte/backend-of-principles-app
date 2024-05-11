using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ReturnForeignKeyForFileEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SecondUserId",
                schema: "flt",
                table: "FileEntities",
                newName: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FileEntities_UserId",
                schema: "flt",
                table: "FileEntities",
                column: "UserId");

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

            migrationBuilder.DropIndex(
                name: "IX_FileEntities_UserId",
                schema: "flt",
                table: "FileEntities");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "flt",
                table: "FileEntities",
                newName: "SecondUserId");
        }
    }
}
