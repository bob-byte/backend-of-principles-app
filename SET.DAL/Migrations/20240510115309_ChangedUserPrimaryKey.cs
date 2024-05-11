using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangedUserPrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileEntities_Users_UserId",
                schema: "flt",
                table: "FileEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAreasOfLife_Users_UserId",
                schema: "arlf",
                table: "UserAreasOfLife");

            migrationBuilder.DropForeignKey(
                name: "FK_UserHabits_Users_UserId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserHabits_UserId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropIndex(
                name: "IX_UserAreasOfLife_UserId",
                schema: "arlf",
                table: "UserAreasOfLife");

            migrationBuilder.DropIndex(
                name: "IX_FileEntities_UserId",
                schema: "flt",
                table: "FileEntities");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "arlf",
                table: "UserAreasOfLife");

            migrationBuilder.CreateSequence(
                name: "SQ_Users",
                startValue: 100L);

            migrationBuilder.AlterColumn<long>(
                name: "Id2",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR SQ_Users",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "UserSecondId",
                schema: "hbt",
                table: "UserHabits",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserId2",
                schema: "arlf",
                table: "UserAreasOfLife",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserSecondId",
                schema: "arlf",
                table: "UserAreasOfLife",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserId2",
                schema: "flt",
                table: "FileEntities",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id2");

            migrationBuilder.CreateIndex(
                name: "IX_UserHabits_UserSecondId",
                schema: "hbt",
                table: "UserHabits",
                column: "UserSecondId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAreasOfLife_UserId2",
                schema: "arlf",
                table: "UserAreasOfLife",
                column: "UserId2");

            migrationBuilder.CreateIndex(
                name: "IX_FileEntities_UserId2",
                schema: "flt",
                table: "FileEntities",
                column: "UserId2");

            migrationBuilder.AddForeignKey(
                name: "FK_FileEntities_Users_UserId2",
                schema: "flt",
                table: "FileEntities",
                column: "UserId2",
                principalTable: "Users",
                principalColumn: "Id2");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAreasOfLife_Users_UserId2",
                schema: "arlf",
                table: "UserAreasOfLife",
                column: "UserId2",
                principalTable: "Users",
                principalColumn: "Id2",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserHabits_Users_UserSecondId",
                schema: "hbt",
                table: "UserHabits",
                column: "UserSecondId",
                principalTable: "Users",
                principalColumn: "Id2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileEntities_Users_UserId2",
                schema: "flt",
                table: "FileEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAreasOfLife_Users_UserId2",
                schema: "arlf",
                table: "UserAreasOfLife");

            migrationBuilder.DropForeignKey(
                name: "FK_UserHabits_Users_UserSecondId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserHabits_UserSecondId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropIndex(
                name: "IX_UserAreasOfLife_UserId2",
                schema: "arlf",
                table: "UserAreasOfLife");

            migrationBuilder.DropIndex(
                name: "IX_FileEntities_UserId2",
                schema: "flt",
                table: "FileEntities");

            migrationBuilder.DropColumn(
                name: "UserSecondId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropColumn(
                name: "UserId2",
                schema: "arlf",
                table: "UserAreasOfLife");

            migrationBuilder.DropColumn(
                name: "UserSecondId",
                schema: "arlf",
                table: "UserAreasOfLife");

            migrationBuilder.DropColumn(
                name: "UserId2",
                schema: "flt",
                table: "FileEntities");

            migrationBuilder.DropSequence(
                name: "SQ_Users");

            migrationBuilder.AlterColumn<long>(
                name: "Id2",
                table: "Users",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValueSql: "NEXT VALUE FOR SQ_Users");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "hbt",
                table: "UserHabits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "arlf",
                table: "UserAreasOfLife",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserHabits_UserId",
                schema: "hbt",
                table: "UserHabits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAreasOfLife_UserId",
                schema: "arlf",
                table: "UserAreasOfLife",
                column: "UserId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_UserAreasOfLife_Users_UserId",
                schema: "arlf",
                table: "UserAreasOfLife",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserHabits_Users_UserId",
                schema: "hbt",
                table: "UserHabits",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
