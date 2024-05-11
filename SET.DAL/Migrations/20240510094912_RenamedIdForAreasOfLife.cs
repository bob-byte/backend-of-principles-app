using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenamedIdForAreasOfLife : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SecondId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "SecondId",
                schema: "arlf",
                table: "UserAreasOfLife",
                newName: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                newName: "SecondId");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "arlf",
                table: "UserAreasOfLife",
                newName: "SecondId");
        }
    }
}
