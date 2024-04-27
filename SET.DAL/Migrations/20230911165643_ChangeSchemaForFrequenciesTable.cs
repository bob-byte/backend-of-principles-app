using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations;

/// <inheritdoc />
public partial class ChangeSchemaForFrequenciesTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "ntc");

        migrationBuilder.RenameTable(
            name: "Frequencies",
            newName: "Frequencies",
            newSchema: "ntc");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameTable(
            name: "Frequencies",
            schema: "ntc",
            newName: "Frequencies");
    }
}
