using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedCreatedAtFieldToClientLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "app",
                table: "ClientLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now() at time zone 'UTC' at time zone 'Europe/Kiev'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "app",
                table: "ClientLogs");
        }
    }
}
