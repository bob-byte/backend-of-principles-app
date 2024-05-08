using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class DeleteExtraModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_DevelopmentPlan_DevelopmentPlanId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "DevelopmentPlan");

            migrationBuilder.DropIndex(
                name: "IX_Users_DevelopmentPlanId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DevelopmentPlanId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FollowedCount",
                schema: "hbt",
                table: "UserHabits");

            migrationBuilder.DropColumn(
                name: "Remind",
                schema: "hbt",
                table: "UserHabits");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DevelopmentPlanId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserType",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FollowedCount",
                schema: "hbt",
                table: "UserHabits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "Remind",
                schema: "hbt",
                table: "UserHabits",
                type: "time",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DevelopmentPlan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DevelopmentPlanType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevelopmentPlan", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_DevelopmentPlanId",
                table: "Users",
                column: "DevelopmentPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_DevelopmentPlan_DevelopmentPlanId",
                table: "Users",
                column: "DevelopmentPlanId",
                principalTable: "DevelopmentPlan",
                principalColumn: "Id");
        }
    }
}
