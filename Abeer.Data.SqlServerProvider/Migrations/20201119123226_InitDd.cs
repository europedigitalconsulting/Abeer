using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Abeer.Data.SqlServerProvider.Migrations
{
    public partial class InitDd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdPrice",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PriceName = table.Column<string>(nullable: true),
                    PriceDescription = table.Column<string>(nullable: true),
                    Value = table.Column<decimal>(nullable: false),
                    DelayToDisplay = table.Column<int>(nullable: false),
                    DisplayDuration = table.Column<int>(nullable: true),
                    MaxViewCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdPrice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Card",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CardType = table.Column<string>(nullable: true),
                    CardNumber = table.Column<string>(nullable: true),
                    PinCode = table.Column<string>(nullable: true),
                    Value = table.Column<int>(nullable: false),
                    IsGenerated = table.Column<bool>(nullable: false),
                    CreatorId = table.Column<string>(nullable: true),
                    CsvFileContent = table.Column<byte[]>(nullable: true),
                    IsUsed = table.Column<bool>(nullable: false),
                    IsSold = table.Column<bool>(nullable: false),
                    SoldDate = table.Column<DateTime>(nullable: true),
                    HasError = table.Column<bool>(nullable: false),
                    ErrorMessage = table.Column<string>(nullable: true),
                    ErrorType = table.Column<int>(nullable: false),
                    IsProcessing = table.Column<bool>(nullable: false),
                    StartProcessing = table.Column<DateTime>(nullable: true),
                    GeneratedDate = table.Column<DateTime>(nullable: false),
                    GeneratedBy = table.Column<string>(nullable: true),
                    Icon = table.Column<string>(nullable: true),
                    SoldBy = table.Column<string>(nullable: true),
                    Quantity = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Card", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    OwnerId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    Gdp = table.Column<string>(nullable: true),
                    Faocode = table.Column<string>(nullable: true),
                    Label = table.Column<string>(nullable: true),
                    Eeacode = table.Column<string>(nullable: true),
                    Estatcode = table.Column<string>(nullable: true),
                    Nutscode = table.Column<string>(nullable: true),
                    Culture = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomLink",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    OwnerId = table.Column<string>(nullable: true),
                    BackgroundColor = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    DisplayInfo = table.Column<string>(nullable: true),
                    Logo = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomLink", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AdId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    PaymentReference = table.Column<string>(nullable: true),
                    PaymentMethod = table.Column<string>(nullable: true),
                    TokenId = table.Column<string>(nullable: true),
                    IsValidated = table.Column<bool>(nullable: false),
                    ValidatedDate = table.Column<DateTime>(nullable: false),
                    PayerID = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SocialNetwork",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    OwnerId = table.Column<string>(nullable: true),
                    BackgroundColor = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    DisplayInfo = table.Column<string>(nullable: true),
                    Logo = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialNetwork", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrlShortned",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(nullable: true),
                    IsSingle = table.Column<bool>(nullable: false),
                    IsSecure = table.Column<bool>(nullable: false),
                    SecureKey = table.Column<string>(nullable: true),
                    IsClicked = table.Column<bool>(nullable: false),
                    ClickedDate = table.Column<DateTime>(nullable: true),
                    LongUrl = table.Column<string>(nullable: true),
                    ShortUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlShortned", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ad",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    OrderNumber = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Price = table.Column<decimal>(nullable: false),
                    Url1 = table.Column<string>(nullable: true),
                    Url2 = table.Column<string>(nullable: true),
                    Url3 = table.Column<string>(nullable: true),
                    Url4 = table.Column<string>(nullable: true),
                    ImageUrl1 = table.Column<string>(nullable: true),
                    ImageUrl2 = table.Column<string>(nullable: true),
                    ImageUrl3 = table.Column<string>(nullable: true),
                    ImageUrl4 = table.Column<string>(nullable: true),
                    OwnerId = table.Column<string>(nullable: true),
                    ViewCount = table.Column<int>(nullable: false),
                    PaymentInformation = table.Column<string>(nullable: true),
                    StartDisplayTime = table.Column<DateTime>(nullable: false),
                    EndDisplayTime = table.Column<DateTime>(nullable: true),
                    IsValid = table.Column<bool>(nullable: false),
                    ValidateDate = table.Column<DateTime>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    AdPriceId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ad_AdPrice_AdPriceId",
                        column: x => x.AdPriceId,
                        principalTable: "AdPrice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardStatu",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CardId = table.Column<Guid>(nullable: false),
                    Status = table.Column<short>(nullable: false),
                    StatusMessage = table.Column<string>(nullable: true),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardStatu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardStatu_Card_CardId",
                        column: x => x.CardId,
                        principalTable: "Card",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ad_AdPriceId",
                table: "Ad",
                column: "AdPriceId");

            migrationBuilder.CreateIndex(
                name: "IX_CardStatu_CardId",
                table: "CardStatu",
                column: "CardId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ad");

            migrationBuilder.DropTable(
                name: "CardStatu");

            migrationBuilder.DropTable(
                name: "Contact");

            migrationBuilder.DropTable(
                name: "Country");

            migrationBuilder.DropTable(
                name: "CustomLink");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "SocialNetwork");

            migrationBuilder.DropTable(
                name: "UrlShortned");

            migrationBuilder.DropTable(
                name: "AdPrice");

            migrationBuilder.DropTable(
                name: "Card");
        }
    }
}
