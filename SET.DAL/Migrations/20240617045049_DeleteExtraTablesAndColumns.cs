using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class DeleteExtraTablesAndColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileEntities",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Statements",
                schema: "app");

            migrationBuilder.DropTable(
                name: "UserHabitUserHabit",
                schema: "hbt");

            migrationBuilder.DropColumn(
                name: "PercentageAchieved",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropColumn(
                name: "ColorName",
                schema: "arlf",
                table: "UserAreasOfLife");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "arlf",
                table: "UserAreasOfLife");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "arlf",
                table: "UserAreasOfLife");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                schema: "hbt",
                table: "ProgressesOfHabits");

            migrationBuilder.DropColumn(
                name: "Value",
                schema: "app",
                table: "Frequencies");

            migrationBuilder.DropSequence(
                name: "sq__file_entities",
                schema: "app");

            migrationBuilder.DropSequence(
                name: "sq__statements",
                schema: "app");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "sq__file_entities",
                schema: "app",
                startValue: 100L);

            migrationBuilder.CreateSequence(
                name: "sq__statements",
                schema: "app",
                startValue: 100L);

            migrationBuilder.AddColumn<decimal>(
                name: "PercentageAchieved",
                schema: "hbt",
                table: "UserHabits",
                type: "numeric(17,16)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ColorName",
                schema: "arlf",
                table: "UserAreasOfLife",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "arlf",
                table: "UserAreasOfLife",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "arlf",
                table: "UserAreasOfLife",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                schema: "hbt",
                table: "ProgressesOfHabits",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Value",
                schema: "app",
                table: "Frequencies",
                type: "numeric(7,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "FileEntities",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('app.sq__file_entities')"),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    FileExtension = table.Column<string>(type: "text", nullable: true),
                    FilePurpose = table.Column<int>(type: "integer", nullable: false),
                    FileType = table.Column<int>(type: "integer", nullable: false),
                    RecordDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileEntities_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "app",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Statements",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('app.sq__statements')"),
                    Author = table.Column<string>(type: "text", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserHabitUserHabit",
                schema: "hbt",
                columns: table => new
                {
                    ParentHabitsId = table.Column<long>(type: "bigint", nullable: false),
                    SubHabitsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHabitUserHabit", x => new { x.ParentHabitsId, x.SubHabitsId });
                    table.ForeignKey(
                        name: "FK_UserHabitUserHabit_UserHabits_ParentHabitsId",
                        column: x => x.ParentHabitsId,
                        principalSchema: "hbt",
                        principalTable: "UserHabits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserHabitUserHabit_UserHabits_SubHabitsId",
                        column: x => x.SubHabitsId,
                        principalSchema: "hbt",
                        principalTable: "UserHabits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileEntities_UserId",
                schema: "app",
                table: "FileEntities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHabitUserHabit_SubHabitsId",
                schema: "hbt",
                table: "UserHabitUserHabit",
                column: "SubHabitsId");
        }
    }
}
