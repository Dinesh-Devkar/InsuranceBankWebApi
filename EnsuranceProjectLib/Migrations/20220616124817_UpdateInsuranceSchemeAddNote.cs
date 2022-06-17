using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnsuranceProjectLib.Migrations
{
    public partial class UpdateInsuranceSchemeAddNote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "InsuranceSchemes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "InsuranceSchemes");
        }
    }
}
