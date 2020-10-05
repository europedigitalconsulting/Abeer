using Microsoft.EntityFrameworkCore.Migrations;

namespace AbeerContactShared.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "test",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "test",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
