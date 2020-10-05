using Microsoft.EntityFrameworkCore.Migrations;

namespace AbeerContactShared.Migrations
{
    public partial class InitCreat1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "test",
                table: "Utilisateurs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "test",
                table: "Utilisateurs");
        }
    }
}
