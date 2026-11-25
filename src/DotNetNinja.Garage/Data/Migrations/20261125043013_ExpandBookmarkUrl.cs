using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetNinja.Garage.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExpandBookmarkUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Href",
                table: "Items",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM [Items] WHERE DATALENGTH([Href]) > 512)
                    THROW 51000, 'Resolve bookmark URLs longer than 256 characters before rolling back.', 1;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Href",
                table: "Items",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048);
        }
    }
}
