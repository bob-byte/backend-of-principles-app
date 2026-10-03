using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class DropUserAreasOfLifeUserHabits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAreasOfLifeUserHabits",
                schema: "arlf");

            migrationBuilder.DropSequence(
                name: "sq__user_areas_of_life_user_habits",
                schema: "arlf");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "sq__user_areas_of_life_user_habits",
                schema: "arlf",
                startValue: 100L);

            migrationBuilder.CreateTable(
                name: "UserAreasOfLifeUserHabits",
                schema: "arlf",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('arlf.sq__user_areas_of_life_user_habits')"),
                    AreaOfLifeId = table.Column<long>(type: "bigint", nullable: false),
                    HabitId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAreasOfLifeUserHabits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAreasOfLifeUserHabits_UserAreasOfLife_AreaOfLifeId",
                        column: x => x.AreaOfLifeId,
                        principalSchema: "arlf",
                        principalTable: "UserAreasOfLife",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAreasOfLifeUserHabits_UserHabits_HabitId",
                        column: x => x.HabitId,
                        principalSchema: "hbt",
                        principalTable: "UserHabits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAreasOfLifeUserHabits_AreaOfLifeId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                column: "AreaOfLifeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAreasOfLifeUserHabits_HabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                column: "HabitId");
        }
    }
}
