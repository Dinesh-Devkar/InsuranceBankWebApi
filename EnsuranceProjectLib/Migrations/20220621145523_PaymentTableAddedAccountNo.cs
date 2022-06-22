using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnsuranceProjectLib.Migrations
{
    public partial class PaymentTableAddedAccountNo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InsuranceAccountNumber",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InsuranceAccountNumber",
                table: "Payments");
        }
    }
}
