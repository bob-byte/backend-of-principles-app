using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedAddtionalIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AreaOfLifeSecondId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SecondId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SecondId",
                schema: "arlf",
                table: "UserAreasOfLife",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreaOfLifeSecondId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropColumn(
                name: "SecondId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropColumn(
                name: "SecondId",
                schema: "arlf",
                table: "UserAreasOfLife");
        }
    }
}
