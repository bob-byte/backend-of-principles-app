using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddClientLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "sq__client_logs",
                schema: "app",
                startValue: 100L);

            migrationBuilder.CreateTable(
                name: "ClientLogs",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('app.sq__client_logs')"),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    DeviceOs = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeviceModelName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeviceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeviceManufacturer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AppVersion = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    LogType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LogMessage = table.Column<string>(type: "text", nullable: false),
                    StackTrace = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientLogs_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "app",
                        principalTable: "Users",
                        principalColumn: "Id");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientLogs",
                schema: "app");

            migrationBuilder.DropSequence(
                name: "sq__client_logs",
                schema: "app");
        }
    }
}
