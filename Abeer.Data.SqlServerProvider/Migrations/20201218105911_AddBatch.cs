using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Abeer.Data.SqlServerProvider.Migrations
{
    public partial class AddBatch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CsvFileContent",
                table: "Card");

            migrationBuilder.AddColumn<Guid>(
                name: "BatchId",
                table: "Card",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Batch",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CardType = table.Column<string>(nullable: true),
                    CardStartNumber = table.Column<string>(nullable: true),
                    CardLastNumber = table.Column<string>(nullable: true),
                    Quantity = table.Column<int>(nullable: false),
                    CsvFileContent = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batch", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Card_BatchId",
                table: "Card",
                column: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Card_Batch_BatchId",
                table: "Card",
                column: "BatchId",
                principalTable: "Batch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Card_Batch_BatchId",
                table: "Card");

            migrationBuilder.DropTable(
                name: "Batch");

            migrationBuilder.DropIndex(
                name: "IX_Card_BatchId",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "Card");

            migrationBuilder.AddColumn<byte[]>(
                name: "CsvFileContent",
                table: "Card",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
