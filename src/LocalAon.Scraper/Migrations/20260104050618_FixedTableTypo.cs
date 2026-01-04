using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalAon.Scraper.Migrations
{
    /// <inheritdoc />
    public partial class FixedTableTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductedItems",
                table: "ProductedItems");

            migrationBuilder.RenameTable(
                name: "ProductedItems",
                newName: "ProductItems");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductItems",
                table: "ProductItems",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductItems",
                table: "ProductItems");

            migrationBuilder.RenameTable(
                name: "ProductItems",
                newName: "ProductedItems");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductedItems",
                table: "ProductedItems",
                column: "Id");
        }
    }
}
