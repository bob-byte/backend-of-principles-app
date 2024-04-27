using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenamePrincipleToHabit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgressesOfPrinciples",
                schema: "prc");

            migrationBuilder.DropTable(
                name: "UserAreasOfLifeUserPrinciples",
                schema: "arlf");

            migrationBuilder.DropTable(
                name: "UserPrincipleUserPrinciple",
                schema: "prc");

            migrationBuilder.DropTable(
                name: "UserPrinciples",
                schema: "prc");

            migrationBuilder.EnsureSchema(
                name: "hbt");

            migrationBuilder.RenameColumn(
                name: "TotalPrinciplesProgress",
                schema: "dev",
                table: "DevProgramsProgresses",
                newName: "TotalHabitsProgress");

            migrationBuilder.CreateTable(
                name: "UserHabits",
                schema: "hbt",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReasonToFollow = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PercentageAchieved = table.Column<decimal>(type: "decimal(17,16)", nullable: false),
                    FrequencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Remind = table.Column<TimeOnly>(type: "time", nullable: true),
                    Complexity = table.Column<int>(type: "int", nullable: false),
                    ColorName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FollowedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHabits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserHabits_Frequencies_FrequencyId",
                        column: x => x.FrequencyId,
                        principalSchema: "ntc",
                        principalTable: "Frequencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserHabits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProgressesOfHabits",
                schema: "hbt",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    HabitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressesOfHabits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressesOfHabits_UserHabits_HabitId",
                        column: x => x.HabitId,
                        principalSchema: "hbt",
                        principalTable: "UserHabits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAreasOfLifeUserHabits",
                schema: "arlf",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AreaOfLifeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HabitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriorityOfHabit = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "UserHabitUserHabit",
                schema: "hbt",
                columns: table => new
                {
                    ParentHabitsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubHabitsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressesOfHabits_HabitId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                column: "HabitId");

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

            migrationBuilder.CreateIndex(
                name: "IX_UserHabits_FrequencyId",
                schema: "hbt",
                table: "UserHabits",
                column: "FrequencyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHabits_UserId",
                schema: "hbt",
                table: "UserHabits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHabitUserHabit_SubHabitsId",
                schema: "hbt",
                table: "UserHabitUserHabit",
                column: "SubHabitsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgressesOfHabits",
                schema: "hbt");

            migrationBuilder.DropTable(
                name: "UserAreasOfLifeUserHabits",
                schema: "arlf");

            migrationBuilder.DropTable(
                name: "UserHabitUserHabit",
                schema: "hbt");

            migrationBuilder.DropTable(
                name: "UserHabits",
                schema: "hbt");

            migrationBuilder.EnsureSchema(
                name: "prc");

            migrationBuilder.RenameColumn(
                name: "TotalHabitsProgress",
                schema: "dev",
                table: "DevProgramsProgresses",
                newName: "TotalPrinciplesProgress");

            migrationBuilder.CreateTable(
                name: "UserPrinciples",
                schema: "prc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FrequencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ColorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Complexity = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FollowedCount = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PercentageAchieved = table.Column<decimal>(type: "decimal(8,5)", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReasonToFollow = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remind = table.Column<TimeOnly>(type: "time", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPrinciples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPrinciples_Frequencies_FrequencyId",
                        column: x => x.FrequencyId,
                        principalSchema: "ntc",
                        principalTable: "Frequencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPrinciples_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProgressesOfPrinciples",
                schema: "prc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrincipleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(10,4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressesOfPrinciples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressesOfPrinciples_UserPrinciples_PrincipleId",
                        column: x => x.PrincipleId,
                        principalSchema: "prc",
                        principalTable: "UserPrinciples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAreasOfLifeUserPrinciples",
                schema: "arlf",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AreaOfLifeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrincipleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriorityOfPrinciple = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAreasOfLifeUserPrinciples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAreasOfLifeUserPrinciples_UserAreasOfLife_AreaOfLifeId",
                        column: x => x.AreaOfLifeId,
                        principalSchema: "arlf",
                        principalTable: "UserAreasOfLife",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAreasOfLifeUserPrinciples_UserPrinciples_PrincipleId",
                        column: x => x.PrincipleId,
                        principalSchema: "prc",
                        principalTable: "UserPrinciples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPrincipleUserPrinciple",
                schema: "prc",
                columns: table => new
                {
                    ParentPrinciplesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubPrinciplesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPrincipleUserPrinciple", x => new { x.ParentPrinciplesId, x.SubPrinciplesId });
                    table.ForeignKey(
                        name: "FK_UserPrincipleUserPrinciple_UserPrinciples_ParentPrinciplesId",
                        column: x => x.ParentPrinciplesId,
                        principalSchema: "prc",
                        principalTable: "UserPrinciples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPrincipleUserPrinciple_UserPrinciples_SubPrinciplesId",
                        column: x => x.SubPrinciplesId,
                        principalSchema: "prc",
                        principalTable: "UserPrinciples",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressesOfPrinciples_PrincipleId",
                schema: "prc",
                table: "ProgressesOfPrinciples",
                column: "PrincipleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAreasOfLifeUserPrinciples_AreaOfLifeId",
                schema: "arlf",
                table: "UserAreasOfLifeUserPrinciples",
                column: "AreaOfLifeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAreasOfLifeUserPrinciples_PrincipleId",
                schema: "arlf",
                table: "UserAreasOfLifeUserPrinciples",
                column: "PrincipleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrinciples_FrequencyId",
                schema: "prc",
                table: "UserPrinciples",
                column: "FrequencyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrinciples_UserId",
                schema: "prc",
                table: "UserPrinciples",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrincipleUserPrinciple_SubPrinciplesId",
                schema: "prc",
                table: "UserPrincipleUserPrinciple",
                column: "SubPrinciplesId");
        }
    }
}
