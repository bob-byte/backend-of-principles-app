using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ReturnedForeignKeyForAreasOfLife : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AreaOfLifeSecondId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                newName: "AreaOfLifeId");

            migrationBuilder.RenameIndex(
                name: "IX_UserAreasOfLifeUserHabits_AreaOfLifeSecondId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                newName: "IX_UserAreasOfLifeUserHabits_AreaOfLifeId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAreasOfLifeUserHabits_UserAreasOfLife_AreaOfLifeId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                column: "AreaOfLifeId",
                principalSchema: "arlf",
                principalTable: "UserAreasOfLife",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAreasOfLifeUserHabits_UserAreasOfLife_AreaOfLifeId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.RenameColumn(
                name: "AreaOfLifeId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                newName: "AreaOfLifeSecondId");

            migrationBuilder.RenameIndex(
                name: "IX_UserAreasOfLifeUserHabits_AreaOfLifeId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                newName: "IX_UserAreasOfLifeUserHabits_AreaOfLifeSecondId");
        }
    }
}
