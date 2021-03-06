using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Abeer.Data.SqlServerProvider.Migrations
{
    public partial class AddNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotificationType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotificationIcon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotificationUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDisplayOnlyOnce = table.Column<bool>(type: "bit", nullable: false),
                    IsDisplayed = table.Column<bool>(type: "bit", nullable: false),
                    DisplayMax = table.Column<int>(type: "int", nullable: false),
                    DisplayCount = table.Column<int>(type: "int", nullable: false),
                    MessageTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastDisplayTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CssClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}
