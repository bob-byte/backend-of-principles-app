using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncChangesSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_UserId",
                schema: "tsk",
                table: "Tasks");

            migrationBuilder.CreateSequence(
                name: "sq__sync_deletions",
                schema: "app",
                startValue: 100L);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "tsk",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "TIMESTAMPTZ '-infinity'");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "tsk",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "TIMESTAMPTZ '-infinity'");

            migrationBuilder.CreateTable(
                name: "SyncDeletions",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('app.sq__sync_deletions')"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EntityId = table.Column<long>(type: "bigint", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncDeletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncDeletions_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "app",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserId_UpdatedAt",
                schema: "tsk",
                table: "Tasks",
                columns: new[] { "UserId", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncDeletions_UserId_DeletedAt",
                schema: "app",
                table: "SyncDeletions",
                columns: new[] { "UserId", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncDeletions_UserId_EntityType_EntityId",
                schema: "app",
                table: "SyncDeletions",
                columns: new[] { "UserId", "EntityType", "EntityId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncDeletions",
                schema: "app");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_UserId_UpdatedAt",
                schema: "tsk",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "tsk",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "tsk",
                table: "Tasks");

            migrationBuilder.DropSequence(
                name: "sq__sync_deletions",
                schema: "app");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserId",
                schema: "tsk",
                table: "Tasks",
                column: "UserId");
        }
    }
}
