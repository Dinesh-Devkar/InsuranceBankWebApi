using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnsuranceProjectLib.Migrations
{
    public partial class PaymentTableAdd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstallmentNumber = table.Column<int>(type: "int", nullable: false),
                    InstallmentDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstallmentAmount = table.Column<double>(type: "float", nullable: false),
                    PaidDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
