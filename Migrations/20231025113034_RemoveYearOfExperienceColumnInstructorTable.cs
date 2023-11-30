using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cursus.Migrations
{
    public partial class RemoveYearOfExperienceColumnInstructorTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "YearOfExperience",
                table: "Instructors");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "YearOfExperience",
                table: "Instructors",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
