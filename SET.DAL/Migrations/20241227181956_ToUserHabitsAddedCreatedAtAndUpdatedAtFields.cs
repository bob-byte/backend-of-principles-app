using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ToUserHabitsAddedCreatedAtAndUpdatedAtFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "hbt",
                table: "UserHabits",
                type: "timestamp",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "hbt",
                table: "UserHabits",
                type: "timestamp",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "hbt",
                table: "UserHabits");
        }
    }
}
