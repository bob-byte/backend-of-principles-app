using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveForeignKeyUserAreaOfLifeUserHabit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAreasOfLifeUserHabits_UserAreasOfLife_AreaOfLifeId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
