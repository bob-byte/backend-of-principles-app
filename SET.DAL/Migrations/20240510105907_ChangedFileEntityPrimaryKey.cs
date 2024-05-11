using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangedFileEntityPrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileEntities_Users_UserId",
                table: "FileEntities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FileEntities",
                table: "FileEntities");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "FileEntities");

            migrationBuilder.EnsureSchema(
                name: "flt");

            migrationBuilder.RenameTable(
                name: "FileEntities",
                newName: "FilesEntities",
                newSchema: "flt");

            migrationBuilder.RenameIndex(
                name: "IX_FileEntities_UserId",
                schema: "flt",
                table: "FilesEntities",
                newName: "IX_FilesEntities_UserId");

            migrationBuilder.CreateSequence(
                name: "SQ_FilesEntity",
                startValue: 100L);

            migrationBuilder.AlterColumn<long>(
                name: "SecondId",
                schema: "flt",
                table: "FilesEntities",
                type: "bigint",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR SQ_FilesEntity",
                oldClrType: typeof(long),
                oldType: "bigint");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FilesEntities_Users_UserId",
                schema: "flt",
                table: "FilesEntities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FilesEntities",
                schema: "flt",
                table: "FilesEntities");

            migrationBuilder.DropSequence(
                name: "SQ_FilesEntity");

            migrationBuilder.RenameTable(
                name: "FilesEntities",
                schema: "flt",
                newName: "FileEntities");

            migrationBuilder.RenameIndex(
                name: "IX_FilesEntities_UserId",
                table: "FileEntities",
                newName: "IX_FileEntities_UserId");

            migrationBuilder.AlterColumn<long>(
                name: "SecondId",
                table: "FileEntities",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValueSql: "NEXT VALUE FOR SQ_FilesEntity");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "FileEntities",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_FileEntities",
                table: "FileEntities",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FileEntities_Users_UserId",
                table: "FileEntities",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
