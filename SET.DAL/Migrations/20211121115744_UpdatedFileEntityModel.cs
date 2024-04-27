using Microsoft.EntityFrameworkCore.Migrations;

namespace SET.DataAccess.Migrations;

public partial class UpdatedFileEntityModel : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "FileExtension",
            table: "FileEntities",
            type: "nvarchar(max)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FileExtension",
            table: "FileEntities");
    }
}
