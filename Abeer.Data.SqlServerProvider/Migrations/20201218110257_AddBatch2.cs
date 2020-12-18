using Microsoft.EntityFrameworkCore.Migrations;

namespace Abeer.Data.SqlServerProvider.Migrations
{
    public partial class AddBatch2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Card_Batch_BatchId",
                table: "Card");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Batch",
                table: "Batch");

            migrationBuilder.RenameTable(
                name: "Batch",
                newName: "Batches");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Batches",
                table: "Batches",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Card_Batches_BatchId",
                table: "Card",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Card_Batches_BatchId",
                table: "Card");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Batches",
                table: "Batches");

            migrationBuilder.RenameTable(
                name: "Batches",
                newName: "Batch");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Batch",
                table: "Batch",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Card_Batch_BatchId",
                table: "Card",
                column: "BatchId",
                principalTable: "Batch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
