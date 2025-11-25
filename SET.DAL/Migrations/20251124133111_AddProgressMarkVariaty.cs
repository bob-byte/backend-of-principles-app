using Microsoft.EntityFrameworkCore.Migrations;

using System;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddProgressMarkVariaty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultProgressValue",
                schema: "hbt",
                table: "UserHabits",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
            name: "DefaultProgressValue",
            schema: "hbt",
            table: "UserHabits" );
        }
    }
}
