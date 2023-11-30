using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cursus.Migrations
{
    public partial class CatalogNameIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Catalogs_Name",
                table: "Catalogs",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Catalogs_Name",
                table: "Catalogs");
        }
    }
}
