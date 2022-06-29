using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnsuranceProjectLib.Migrations
{
    public partial class AddedCommitionTypeInCommissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommissionType",
                table: "CommissionRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommissionType",
                table: "CommissionRecords");
        }
    }
}
