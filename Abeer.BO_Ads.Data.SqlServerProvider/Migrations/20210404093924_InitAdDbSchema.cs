using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Abeer.Ads.Data.SqlServerProvider.Migrations
{
    public partial class InitAdDbSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdsFamilies",
                columns: table => new
                {
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PictureUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LabelSearch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeaderAuthorize = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UrlApi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaKeywords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PurchaseLabelRule = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdsFamilies", x => x.FamilyId);
                });

            migrationBuilder.CreateTable(
                name: "CategoryAds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryAds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdsCategories",
                columns: table => new
                {
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaKeywords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PictureUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdsCategories", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_AdsCategories_AdsFamilies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "AdsFamilies",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdsFamilyAttributes",
                columns: table => new
                {
                    FamilyAttributeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsSearchable = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdsFamilyAttributes", x => x.FamilyAttributeId);
                    table.ForeignKey(
                        name: "FK_AdsFamilyAttributes_AdsFamilies_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "AdsFamilies",
                        principalColumn: "FamilyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdsCategories_FamilyId",
                table: "AdsCategories",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_AdsFamilyAttributes_FamilyId",
                table: "AdsFamilyAttributes",
                column: "FamilyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdsCategories");

            migrationBuilder.DropTable(
                name: "AdsFamilyAttributes");

            migrationBuilder.DropTable(
                name: "CategoryAds");

            migrationBuilder.DropTable(
                name: "AdsFamilies");
        }
    }
}
