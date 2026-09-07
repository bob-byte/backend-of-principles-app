using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskSubtasksTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskSubtasks",
                schema: "tsk",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskId = table.Column<long>(type: "bigint", nullable: false),
                    ClientId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskSubtasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskSubtasks_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalSchema: "tsk",
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskSubtasks_TaskId",
                schema: "tsk",
                table: "TaskSubtasks",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskSubtasks_TaskId_ClientId",
                schema: "tsk",
                table: "TaskSubtasks",
                columns: new[] { "TaskId", "ClientId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskSubtasks",
                schema: "tsk");
        }
    }
}
