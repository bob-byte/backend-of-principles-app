using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SET.DataAccess.Migrations;

/// <inheritdoc />
public partial class ChangePasswordColumnToVarBinaryInUserTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Password", table: "Users", schema: "dbo");
        migrationBuilder.AddColumn<byte[]>(
            name: "Password", 
            table: "Users", 
            schema: "dbo",
            type: "varbinary(255)", 
            maxLength: 255, 
            nullable: false);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Challenges",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(255)",
            oldNullable: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Password", table: "Users", schema: "dbo");
        migrationBuilder.AddColumn<string>(name: "Password", table: "Users", schema: "dbo", type: "nvarchar(max)", nullable: false);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Challenges",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(255)",
            oldMaxLength: 255);
    }
}
