using Microsoft.EntityFrameworkCore.Migrations;

namespace SET.DataAccess.Migrations;

public partial class AddPropUserStatusInChallengeToChallangeModel : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "UserStatusInChallenge",
            table: "Challenges",
            type: "int",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "UserStatusInChallenge",
            table: "Challenges");
    }
}
