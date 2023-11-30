using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cursus.Migrations
{
    public partial class UpdateLesson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Lessons");

            migrationBuilder.RenameColumn(
                name: "VideoURL",
                table: "Lessons",
                newName: "Overview");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Lessons",
                newName: "Content");

            migrationBuilder.AddColumn<Guid>(
                name: "SectionID",
                table: "Lessons",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SectionID",
                table: "Lessons");

            migrationBuilder.RenameColumn(
                name: "Overview",
                table: "Lessons",
                newName: "VideoURL");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Lessons",
                newName: "Description");

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Lessons",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
