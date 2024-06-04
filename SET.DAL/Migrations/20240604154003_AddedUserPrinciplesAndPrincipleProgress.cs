using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserPrinciplesAndPrincipleProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "prc");

            migrationBuilder.CreateSequence(
                name: "sq__principle_progresses",
                schema: "prc",
                startValue: 100L);

            migrationBuilder.CreateSequence(
                name: "sq__user_principles",
                schema: "prc",
                startValue: 100L);

            migrationBuilder.CreateTable(
                name: "UserPrinciples",
                schema: "prc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('prc.sq__user_principles')"),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Exceptions = table.Column<string>(type: "text", nullable: true),
                    ReasonToFollow = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPrinciples", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrincipleProgresses",
                schema: "prc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('prc.sq__principle_progresses')"),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    PrincipleId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrincipleProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrincipleProgresses_UserPrinciples_PrincipleId",
                        column: x => x.PrincipleId,
                        principalSchema: "prc",
                        principalTable: "UserPrinciples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrincipleProgresses_PrincipleId",
                schema: "prc",
                table: "PrincipleProgresses",
                column: "PrincipleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrincipleProgresses",
                schema: "prc");

            migrationBuilder.DropTable(
                name: "UserPrinciples",
                schema: "prc");

            migrationBuilder.DropSequence(
                name: "sq__principle_progresses",
                schema: "prc");

            migrationBuilder.DropSequence(
                name: "sq__user_principles",
                schema: "prc");
        }
    }
}
