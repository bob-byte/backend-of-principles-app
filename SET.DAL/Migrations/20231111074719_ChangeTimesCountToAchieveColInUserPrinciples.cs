using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTimesCountToAchieveColInUserPrinciples : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimesCountToAchieve",
                schema: "prc",
                table: "UserPrinciples");

            migrationBuilder.AddColumn<int>(
                name: "Complexity",
                schema: "prc",
                table: "UserPrinciples",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Complexity",
                schema: "prc",
                table: "UserPrinciples");

            migrationBuilder.AddColumn<int>(
                name: "TimesCountToAchieve",
                schema: "prc",
                table: "UserPrinciples",
                type: "int",
                nullable: false,
                defaultValue: 100);
        }
    }
}
