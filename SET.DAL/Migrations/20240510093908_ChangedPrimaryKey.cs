using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangedPrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAreasOfLifeUserHabits",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropIndex(
                name: "IX_UserAreasOfLifeUserHabits_AreaOfLifeId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAreasOfLife",
                schema: "arlf",
                table: "UserAreasOfLife");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropColumn(
                name: "AreaOfLifeId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "arlf",
                table: "UserAreasOfLife");

            migrationBuilder.CreateSequence(
                name: "SQ_UserAreasOfLife",
                startValue: 100L);

            migrationBuilder.CreateSequence(
                name: "SQ_UserAreasOfLifeUserHabit",
                startValue: 100L);

            migrationBuilder.AlterColumn<long>(
                name: "SecondId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                type: "bigint",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR SQ_UserAreasOfLifeUserHabit",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "SecondId",
                schema: "arlf",
                table: "UserAreasOfLife",
                type: "bigint",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR SQ_UserAreasOfLife",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAreasOfLifeUserHabits",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                column: "SecondId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAreasOfLife",
                schema: "arlf",
                table: "UserAreasOfLife",
                column: "SecondId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAreasOfLifeUserHabits_AreaOfLifeSecondId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                column: "AreaOfLifeSecondId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAreasOfLifeUserHabits",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropIndex(
                name: "IX_UserAreasOfLifeUserHabits_AreaOfLifeSecondId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAreasOfLife",
                schema: "arlf",
                table: "UserAreasOfLife");

            migrationBuilder.DropSequence(
                name: "SQ_UserAreasOfLife");

            migrationBuilder.DropSequence(
                name: "SQ_UserAreasOfLifeUserHabit");

            migrationBuilder.AlterColumn<long>(
                name: "SecondId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValueSql: "NEXT VALUE FOR SQ_UserAreasOfLifeUserHabit");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AreaOfLifeId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<long>(
                name: "SecondId",
                schema: "arlf",
                table: "UserAreasOfLife",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValueSql: "NEXT VALUE FOR SQ_UserAreasOfLife");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "arlf",
                table: "UserAreasOfLife",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAreasOfLifeUserHabits",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAreasOfLife",
                schema: "arlf",
                table: "UserAreasOfLife",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserAreasOfLifeUserHabits_AreaOfLifeId",
                schema: "arlf",
                table: "UserAreasOfLifeUserHabits",
                column: "AreaOfLifeId");
        }
    }
}
