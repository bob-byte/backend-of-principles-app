using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations;

/// <inheritdoc />
public partial class ChangeSchemaOfUserAreasOfLifeTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "arlf");

        migrationBuilder.RenameTable(
            name: "UserAreasOfLife",
            newName: "UserAreasOfLife",
            newSchema: "arlf");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameTable(
            name: "UserAreasOfLife",
            schema: "arlf",
            newName: "UserAreasOfLife");
    }
}
