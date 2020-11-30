using Microsoft.EntityFrameworkCore.Migrations;

namespace Abeer.Data.SqlServerProvider.Migrations
{
    public partial class add_column_order_number : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "Payment",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "Payment");
        }
    }
}
