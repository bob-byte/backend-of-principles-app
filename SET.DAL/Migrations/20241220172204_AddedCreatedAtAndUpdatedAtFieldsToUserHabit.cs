using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedCreatedAtAndUpdatedAtFieldsToUserHabit : Migration
    {
        /// <inheritdoc />
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "hbt",
                table: "UserHabits",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime( 1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc ) );

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "hbt",
                table: "UserHabits",
                type: "timestamp with time zone",
                nullable: true );
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "hbt",
                table: "UserHabits" );

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "hbt",
                table: "UserHabits" );
        }
    }
}
