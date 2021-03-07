using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Abeer.Data.SqlServerProvider.Migrations
{
    public partial class AddInvitations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Invitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OwnedId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvitationStat = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invitations", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invitations");
        }
    }
}
