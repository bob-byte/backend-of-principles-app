using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangedPrimaryKeysForHabits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgressesOfHabits_UserHabits_HabitId",
                schema: "hbt",
                table: "ProgressesOfHabits");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAreasOfLifeUserHabits_UserHabits_HabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropForeignKey(
                name: "FK_UserHabits_Frequencies_FrequencyId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropTable(
                name: "UserHabitUserHabit",
                schema: "hbt");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserHabits",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropIndex(
                name: "IX_UserHabits_FrequencyId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProgressesOfHabits",
                schema: "hbt",
                table: "ProgressesOfHabits");

            migrationBuilder.DropIndex(
                name: "IX_ProgressesOfHabits_HabitId",
                schema: "hbt",
                table: "ProgressesOfHabits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Frequencies",
                schema: "ntc",
                table: "Frequencies");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "hbt",
                table: "ProgressesOfHabits");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "ntc",
                table: "Frequencies");

            migrationBuilder.CreateSequence(
                name: "SQ_Frequencies",
                startValue: 100L);

            migrationBuilder.CreateSequence(
                name: "SQ_ProgressesOfHabits",
                startValue: 100L);

            migrationBuilder.CreateSequence(
                name: "SQ_UserHabits",
                startValue: 100L);

            migrationBuilder.AlterColumn<long>(
                name: "SecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabits",
                type: "bigint",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR SQ_UserHabits",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "FrequencySecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabits",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserHabitSecondIdEntityWithId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "SecondIdEntityWithId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                type: "bigint",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR SQ_ProgressesOfHabits",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "HabitSecondIdEntityWithId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<long>(
                name: "SecondIdEntityWithId",
                schema: "ntc",
                table: "Frequencies",
                type: "bigint",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR SQ_Frequencies",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserHabits",
                schema: "hbt",
                table: "UserHabits",
                column: "SecondIdEntityWithId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProgressesOfHabits",
                schema: "hbt",
                table: "ProgressesOfHabits",
                column: "SecondIdEntityWithId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Frequencies",
                schema: "ntc",
                table: "Frequencies",
                column: "SecondIdEntityWithId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHabits_FrequencySecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabits",
                column: "FrequencySecondIdEntityWithId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAreasOfLifeUserHabits_UserHabitSecondIdEntityWithId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                column: "UserHabitSecondIdEntityWithId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressesOfHabits_HabitSecondIdEntityWithId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                column: "HabitSecondIdEntityWithId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressesOfHabits_UserHabits_HabitSecondIdEntityWithId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                column: "HabitSecondIdEntityWithId",
                principalSchema: "hbt",
                principalTable: "UserHabits",
                principalColumn: "SecondIdEntityWithId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAreasOfLifeUserHabits_UserHabits_UserHabitSecondIdEntityWithId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                column: "UserHabitSecondIdEntityWithId",
                principalSchema: "hbt",
                principalTable: "UserHabits",
                principalColumn: "SecondIdEntityWithId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserHabits_Frequencies_FrequencySecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabits",
                column: "FrequencySecondIdEntityWithId",
                principalSchema: "ntc",
                principalTable: "Frequencies",
                principalColumn: "SecondIdEntityWithId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgressesOfHabits_UserHabits_HabitSecondIdEntityWithId",
                schema: "hbt",
                table: "ProgressesOfHabits");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAreasOfLifeUserHabits_UserHabits_UserHabitSecondIdEntityWithId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropForeignKey(
                name: "FK_UserHabits_Frequencies_FrequencySecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserHabits",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropIndex(
                name: "IX_UserHabits_FrequencySecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropIndex(
                name: "IX_UserAreasOfLifeUserHabits_UserHabitSecondIdEntityWithId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProgressesOfHabits",
                schema: "hbt",
                table: "ProgressesOfHabits");

            migrationBuilder.DropIndex(
                name: "IX_ProgressesOfHabits_HabitSecondIdEntityWithId",
                schema: "hbt",
                table: "ProgressesOfHabits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Frequencies",
                schema: "ntc",
                table: "Frequencies");

            migrationBuilder.DropColumn(
                name: "FrequencySecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropColumn(
                name: "UserHabitSecondIdEntityWithId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropColumn(
                name: "HabitSecondIdEntityWithId",
                schema: "hbt",
                table: "ProgressesOfHabits");

            migrationBuilder.DropSequence(
                name: "SQ_Frequencies");

            migrationBuilder.DropSequence(
                name: "SQ_ProgressesOfHabits");

            migrationBuilder.DropSequence(
                name: "SQ_UserHabits");

            migrationBuilder.AlterColumn<long>(
                name: "SecondIdEntityWithId",
                schema: "hbt",
                table: "UserHabits",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValueSql: "NEXT VALUE FOR SQ_UserHabits");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "hbt",
                table: "UserHabits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<long>(
                name: "SecondIdEntityWithId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValueSql: "NEXT VALUE FOR SQ_ProgressesOfHabits");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "hbt",
                table: "ProgressesOfHabits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<long>(
                name: "SecondIdEntityWithId",
                schema: "ntc",
                table: "Frequencies",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValueSql: "NEXT VALUE FOR SQ_Frequencies");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "ntc",
                table: "Frequencies",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserHabits",
                schema: "hbt",
                table: "UserHabits",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProgressesOfHabits",
                schema: "hbt",
                table: "ProgressesOfHabits",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Frequencies",
                schema: "ntc",
                table: "Frequencies",
                column: "Id");

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
                name: "IX_UserHabits_FrequencyId",
                schema: "hbt",
                table: "UserHabits",
                column: "FrequencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressesOfHabits_HabitId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                column: "HabitId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHabitUserHabit_SubHabitsId",
                schema: "hbt",
                table: "UserHabitUserHabit",
                column: "SubHabitsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressesOfHabits_UserHabits_HabitId",
                schema: "hbt",
                table: "ProgressesOfHabits",
                column: "HabitId",
                principalSchema: "hbt",
                principalTable: "UserHabits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAreasOfLifeUserHabits_UserHabits_HabitId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                column: "HabitId",
                principalSchema: "hbt",
                principalTable: "UserHabits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserHabits_Frequencies_FrequencyId",
                schema: "hbt",
                table: "UserHabits",
                column: "FrequencyId",
                principalSchema: "ntc",
                principalTable: "Frequencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
