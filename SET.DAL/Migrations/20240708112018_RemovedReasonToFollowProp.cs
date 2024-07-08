using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemovedReasonToFollowProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReasonToFollow",
                schema: "hbt",
                table: "UserHabits");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReasonToFollow",
                schema: "hbt",
                table: "UserHabits",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
