using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalAon.Scraper.Migrations
{
    /// <inheritdoc />
    public partial class Bloodlines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bloodlines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ClassSkill = table.Column<string>(type: "TEXT", nullable: true),
                    BonusSpells = table.Column<string>(type: "TEXT", nullable: true),
                    BonusFeats = table.Column<string>(type: "TEXT", nullable: true),
                    BloodlineArcana = table.Column<string>(type: "TEXT", nullable: true),
                    BloodlinePowers = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bloodlines", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bloodlines");
        }
    }
}
