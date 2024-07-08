using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserGoalsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "goal");

            migrationBuilder.CreateSequence(
                name: "sq__user_goals",
                schema: "goal",
                startValue: 100L);

            migrationBuilder.AddColumn<long>(
                name: "GoalId",
                schema: "hbt",
                table: "UserHabits",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserGoals",
                schema: "goal",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('goal.sq__user_goals')"),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGoals_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "app",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserHabits_GoalId",
                schema: "hbt",
                table: "UserHabits",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGoals_UserId",
                schema: "goal",
                table: "UserGoals",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserHabits_UserGoals_GoalId",
                schema: "hbt",
                table: "UserHabits",
                column: "GoalId",
                principalSchema: "goal",
                principalTable: "UserGoals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserHabits_UserGoals_GoalId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropTable(
                name: "UserGoals",
                schema: "goal");

            migrationBuilder.DropIndex(
                name: "IX_UserHabits_GoalId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropColumn(
                name: "GoalId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropSequence(
                name: "sq__user_goals",
                schema: "goal");
        }
    }
}
