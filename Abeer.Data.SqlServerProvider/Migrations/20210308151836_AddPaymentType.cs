using Microsoft.EntityFrameworkCore.Migrations;

namespace Abeer.Data.SqlServerProvider.Migrations
{
    public partial class AddPaymentType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AdId",
                table: "Payment",
                newName: "Reference");

            migrationBuilder.AddColumn<string>(
                name: "PaymentType",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "Payment");

            migrationBuilder.RenameColumn(
                name: "Reference",
                table: "Payment",
                newName: "AdId");
        }
    }
}
