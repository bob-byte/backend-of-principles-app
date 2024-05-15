using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateInPostreSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "flt");

            migrationBuilder.EnsureSchema(
                name: "hbt");

            migrationBuilder.EnsureSchema(
                name: "stm");

            migrationBuilder.EnsureSchema(
                name: "arlf");

            migrationBuilder.EnsureSchema(
                name: "usr");

            migrationBuilder.CreateSequence(
                name: "sq_filesentity",
                startValue: 100L);

            migrationBuilder.CreateSequence(
                name: "sq_frequencies",
                startValue: 100L);

            migrationBuilder.CreateSequence(
                name: "sq_progressesofhabits",
                startValue: 100L);

            migrationBuilder.CreateSequence(
                name: "sq_statements",
                startValue: 100L);

            migrationBuilder.CreateSequence(
                name: "sq_userareasoflife",
                startValue: 100L);

            migrationBuilder.CreateSequence(
                name: "sq_userareasoflifeuserhabit",
                startValue: 100L);

            migrationBuilder.CreateSequence(
                name: "sq_userhabits",
                startValue: 100L);

            migrationBuilder.CreateSequence(
                name: "sq_users",
                startValue: 100L);

            migrationBuilder.CreateTable(
                name: "Frequencies",
                schema: "hbt",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('sq_frequencies')"),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(7,6)", nullable: false),
                    Repeats = table.Column<int>(type: "integer", nullable: false),
                    IntervalLengthInDays = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frequencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Statements",
                schema: "stm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('sq_statements')"),
                    Author = table.Column<string>(type: "text", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "usr",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('sq_users')"),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<byte[]>(type: "bytea", maxLength: 255, nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    MainSlogan = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Mission = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileEntities",
                schema: "flt",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('sq_filesentity')"),
                    FileExtension = table.Column<string>(type: "text", nullable: true),
                    FileType = table.Column<int>(type: "integer", nullable: false),
                    FilePurpose = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    RecordDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileEntities_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "usr",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserAreasOfLife",
                schema: "arlf",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('sq_userareasoflife')"),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ColorName = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAreasOfLife", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAreasOfLife_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "usr",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserHabits",
                schema: "hbt",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('sq_userhabits')"),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ReasonToFollow = table.Column<string>(type: "text", nullable: false),
                    Question = table.Column<string>(type: "text", nullable: true),
                    PercentageAchieved = table.Column<decimal>(type: "numeric(17,16)", nullable: false),
                    FrequencyId = table.Column<long>(type: "bigint", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Complexity = table.Column<int>(type: "integer", nullable: false),
                    ColorName = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHabits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserHabits_Frequencies_FrequencyId",
                        column: x => x.FrequencyId,
                        principalSchema: "hbt",
                        principalTable: "Frequencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserHabits_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "usr",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProgressesOfHabits",
                schema: "hbt",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('sq_progressesofhabits')"),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    HabitId = table.Column<long>(type: "bigint", nullable: false)
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
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('sq_userareasoflifeuserhabit')"),
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
                schema: "flt",
                table: "FileEntities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressesOfHabits_HabitId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                column: "HabitId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAreasOfLife_UserId",
                schema: "arlf",
                table: "UserAreasOfLife",
                column: "UserId");

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
                name: "FileEntities",
                schema: "flt");

            migrationBuilder.DropTable(
                name: "ProgressesOfHabits",
                schema: "hbt");

            migrationBuilder.DropTable(
                name: "Statements",
                schema: "stm");

            migrationBuilder.DropTable(
                name: "UserAreasOfLifeUserHabits",
                schema: "arlf");

            migrationBuilder.DropTable(
                name: "UserHabitUserHabit",
                schema: "hbt");

            migrationBuilder.DropTable(
                name: "UserAreasOfLife",
                schema: "arlf");

            migrationBuilder.DropTable(
                name: "UserHabits",
                schema: "hbt");

            migrationBuilder.DropTable(
                name: "Frequencies",
                schema: "hbt");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "usr");

            migrationBuilder.DropSequence(
                name: "sq_filesentity");

            migrationBuilder.DropSequence(
                name: "sq_frequencies");

            migrationBuilder.DropSequence(
                name: "sq_progressesofhabits");

            migrationBuilder.DropSequence(
                name: "sq_statements");

            migrationBuilder.DropSequence(
                name: "sq_userareasoflife");

            migrationBuilder.DropSequence(
                name: "sq_userareasoflifeuserhabit");

            migrationBuilder.DropSequence(
                name: "sq_userhabits");

            migrationBuilder.DropSequence(
                name: "sq_users");
        }
    }
}
