using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetNinja.Garage.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEditConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_LinkGroupId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Groups_LinkPageId",
                table: "Groups");

            migrationBuilder.AddColumn<int>(
                name: "Revision",
                table: "Pages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Revision",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Revision",
                table: "Groups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Pages_UserId_Slug",
                table: "Pages",
                columns: new[] { "UserId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pages_UserId_SortIndex",
                table: "Pages",
                columns: new[] { "UserId", "SortIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_LinkGroupId_SortIndex",
                table: "Items",
                columns: new[] { "LinkGroupId", "SortIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_Groups_LinkPageId_SortIndex",
                table: "Groups",
                columns: new[] { "LinkPageId", "SortIndex" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pages_UserId_Slug",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Pages_UserId_SortIndex",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Items_LinkGroupId_SortIndex",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Groups_LinkPageId_SortIndex",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "Revision",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "Revision",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Revision",
                table: "Groups");

            migrationBuilder.CreateIndex(
                name: "IX_Items_LinkGroupId",
                table: "Items",
                column: "LinkGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_LinkPageId",
                table: "Groups",
                column: "LinkPageId");
        }
    }
}
