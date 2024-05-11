using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenamedUserSecondIdToId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropIndex(
                name: "IX_UserAreasOfLife_UserId2",
                schema: "arlf",
                table: "UserAreasOfLife");

            migrationBuilder.DropColumn(
                name: "UserId2",
                schema: "arlf",
                table: "UserAreasOfLife");

            migrationBuilder.RenameColumn(
                name: "Id2",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "UserSecondId",
                schema: "hbt",
                table: "UserHabits",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserHabits_UserSecondId",
                schema: "hbt",
                table: "UserHabits",
                newName: "IX_UserHabits_UserId");

            migrationBuilder.RenameColumn(
                name: "UserSecondId",
                schema: "arlf",
                table: "UserAreasOfLife",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "UserId2",
                schema: "flt",
                table: "FileEntities",
                newName: "UserId1");

            migrationBuilder.RenameIndex(
                name: "IX_FileEntities_UserId2",
                schema: "flt",
                table: "FileEntities",
                newName: "IX_FileEntities_UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserAreasOfLife_UserId",
                schema: "arlf",
                table: "UserAreasOfLife",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileEntities_Users_UserId1",
                schema: "flt",
                table: "FileEntities",
                column: "UserId1",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileEntities_Users_UserId1",
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

            migrationBuilder.DropIndex(
                name: "IX_UserAreasOfLife_UserId",
                schema: "arlf",
                table: "UserAreasOfLife");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "Id2");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "hbt",
                table: "UserHabits",
                newName: "UserSecondId");

            migrationBuilder.RenameIndex(
                name: "IX_UserHabits_UserId",
                schema: "hbt",
                table: "UserHabits",
                newName: "IX_UserHabits_UserSecondId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "arlf",
                table: "UserAreasOfLife",
                newName: "UserSecondId");

            migrationBuilder.RenameColumn(
                name: "UserId1",
                schema: "flt",
                table: "FileEntities",
                newName: "UserId2");

            migrationBuilder.RenameIndex(
                name: "IX_FileEntities_UserId1",
                schema: "flt",
                table: "FileEntities",
                newName: "IX_FileEntities_UserId2");

            migrationBuilder.AddColumn<long>(
                name: "UserId2",
                schema: "arlf",
                table: "UserAreasOfLife",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_UserAreasOfLife_UserId2",
                schema: "arlf",
                table: "UserAreasOfLife",
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
    }
}
