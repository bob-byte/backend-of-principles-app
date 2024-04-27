using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations;

/// <inheritdoc />
public partial class AddColumnNameToUsersTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FirstName",
            table: "Users");

        migrationBuilder.RenameColumn(
            name: "LastName",
            table: "Users",
            newName: "Name");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "Name",
            table: "Users",
            newName: "LastName");

        migrationBuilder.AddColumn<string>(
            name: "FirstName",
            table: "Users",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");
    }
}
