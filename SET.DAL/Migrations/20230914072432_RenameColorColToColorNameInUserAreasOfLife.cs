using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations;

/// <inheritdoc />
public partial class RenameColorColToColorNameInUserAreasOfLife : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "Color",
            schema: "arlf",
            table: "UserAreasOfLife",
            newName: "ColorName");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "ColorName",
            schema: "arlf",
            table: "UserAreasOfLife",
            newName: "Color");
    }
}
