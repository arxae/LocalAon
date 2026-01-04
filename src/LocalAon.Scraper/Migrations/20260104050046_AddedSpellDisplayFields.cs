using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalAon.Scraper.Migrations
{
    /// <inheritdoc />
    public partial class AddedSpellDisplayFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "Spells",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SavingThrow",
                table: "Spells",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpellResistance",
                table: "Spells",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "Spells");

            migrationBuilder.DropColumn(
                name: "SavingThrow",
                table: "Spells");

            migrationBuilder.DropColumn(
                name: "SpellResistance",
                table: "Spells");
        }
    }
}
