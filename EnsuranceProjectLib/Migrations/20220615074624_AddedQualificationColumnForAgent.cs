using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnsuranceProjectLib.Migrations
{
    public partial class AddedQualificationColumnForAgent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Qualification",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Qualification",
                table: "AspNetUsers");
        }
    }
}
