using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemovedCmplicatedDeProgramsAndDevProgramsProgresses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplicatedDevPrograms",
                schema: "dev");

            migrationBuilder.DropTable(
                name: "DevProgramsProgresses",
                schema: "dev");

            migrationBuilder.DropTable(
                name: "TypeOfComplicatedDevProgram");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dev");

            migrationBuilder.CreateTable(
                name: "DevProgramsProgresses",
                schema: "dev",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangedInPercent = table.Column<decimal>(type: "decimal(11,8)", nullable: false),
                    TotalHabitsProgress = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    TotalProgressOfGoals = table.Column<decimal>(type: "decimal(18,8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevProgramsProgresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TypeOfComplicatedDevProgram",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeOfComplicatedDevProgram", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComplicatedDevPrograms",
                schema: "dev",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FullDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplicatedDevPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplicatedDevPrograms_TypeOfComplicatedDevProgram_TypeId",
                        column: x => x.TypeId,
                        principalTable: "TypeOfComplicatedDevProgram",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComplicatedDevPrograms_TypeId",
                schema: "dev",
                table: "ComplicatedDevPrograms",
                column: "TypeId");
        }
    }
}
