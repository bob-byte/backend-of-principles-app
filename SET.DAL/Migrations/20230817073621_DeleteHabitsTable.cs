using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations;

/// <inheritdoc />
public partial class DeleteHabitsTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Notices_Habits_HabitId",
            table: "Notices");

        migrationBuilder.DropTable(
            name: "Habits");

        migrationBuilder.DropIndex(
            name: "IX_Notices_HabitId",
            table: "Notices");

        migrationBuilder.DropColumn(
            name: "HabitId",
            table: "Notices");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "HabitId",
            table: "Notices",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.CreateTable(
            name: "Habits",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AcquiredUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                NegativeUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                OvercomeNegativeUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                PositiveUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ActivityArea = table.Column<int>(type: "int", nullable: false),
                DaysCountToAchieve = table.Column<int>(type: "int", nullable: false),
                DescriptionUri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                TimesCountExecutedAct = table.Column<int>(type: "int", nullable: false),
                TypeOfHabit = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Habits", x => x.Id);
                table.ForeignKey(
                    name: "FK_Habits_Users_AcquiredUserId",
                    column: x => x.AcquiredUserId,
                    principalTable: "Users",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Habits_Users_NegativeUserId",
                    column: x => x.NegativeUserId,
                    principalTable: "Users",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Habits_Users_OvercomeNegativeUserId",
                    column: x => x.OvercomeNegativeUserId,
                    principalTable: "Users",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Habits_Users_PositiveUserId",
                    column: x => x.PositiveUserId,
                    principalTable: "Users",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Notices_HabitId",
            table: "Notices",
            column: "HabitId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Habits_AcquiredUserId",
            table: "Habits",
            column: "AcquiredUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Habits_NegativeUserId",
            table: "Habits",
            column: "NegativeUserId",
            unique: true,
            filter: "[NegativeUserId] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Habits_OvercomeNegativeUserId",
            table: "Habits",
            column: "OvercomeNegativeUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Habits_PositiveUserId",
            table: "Habits",
            column: "PositiveUserId");

        migrationBuilder.AddForeignKey(
            name: "FK_Notices_Habits_HabitId",
            table: "Notices",
            column: "HabitId",
            principalTable: "Habits",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
