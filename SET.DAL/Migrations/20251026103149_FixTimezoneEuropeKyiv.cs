using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixTimezoneEuropeKyiv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "app",
                table: "ClientLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now() at time zone 'Europe/Kyiv'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now() at time zone 'Europe/Kiev'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "app",
                table: "ClientLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now() at time zone 'Europe/Kiev'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now() at time zone 'Europe/Kyiv'");
        }
    }
}
