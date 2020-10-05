using Microsoft.EntityFrameworkCore.Migrations;

namespace AbeerContactShared.Migrations
{
    public partial class InitCreat3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Utilisateurs",
                table: "Utilisateurs");

            migrationBuilder.RenameTable(
                name: "Utilisateurs",
                newName: "Users");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "UserId");

            migrationBuilder.CreateTable(
                name: "IdentityUserClaim",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(nullable: true),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityUserClaim", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdentityUserClaim");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Utilisateurs");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Utilisateurs",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Utilisateurs",
                table: "Utilisateurs",
                column: "Id");
        }
    }
}
