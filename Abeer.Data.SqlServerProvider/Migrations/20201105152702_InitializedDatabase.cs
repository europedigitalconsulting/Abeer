using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Abeer.Data.SqlServerProvider.Migrations
{
    public partial class InitializedDatabase : Migration
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
                name: "TokenBatch",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TokenType = table.Column<string>(nullable: true),
                    BatchNumber = table.Column<string>(nullable: true),
                    PartsItemsCount = table.Column<int>(nullable: false),
                    OperatorId = table.Column<string>(nullable: true),
                    OperatorName = table.Column<string>(nullable: true),
                    IsGenerated = table.Column<bool>(nullable: false),
                    CsvFileContent = table.Column<byte[]>(nullable: true),
                    IsError = table.Column<bool>(nullable: false),
                    ErrorType = table.Column<int>(nullable: false),
                    ErrorMessage = table.Column<string>(nullable: true),
                    WebHookUrl = table.Column<string>(nullable: true),
                    WebHookAuthentication = table.Column<string>(nullable: true),
                    WebHookProtocolType = table.Column<string>(nullable: true),
                    GeneratedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenBatch", x => x.Id);
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
                name: "Wallet",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    OwnerId = table.Column<string>(nullable: true),
                    Identifier = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ad",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    OrderNumber = table.Column<string>(nullable: true),
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

            migrationBuilder.CreateTable(
                name: "TokenBatchStatu",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TokenBatchId = table.Column<Guid>(nullable: false),
                    TokenBatchId1 = table.Column<long>(nullable: true),
                    Status = table.Column<short>(nullable: false),
                    StatusMessage = table.Column<string>(nullable: true),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenBatchStatu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenBatchStatu_TokenBatch_TokenBatchId1",
                        column: x => x.TokenBatchId1,
                        principalTable: "TokenBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TokenItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PartNumber = table.Column<string>(nullable: true),
                    IsUsed = table.Column<bool>(nullable: false),
                    UsedBy = table.Column<string>(nullable: true),
                    PartPosition = table.Column<long>(nullable: false),
                    PinCode = table.Column<string>(nullable: true),
                    TokenBatchId = table.Column<long>(nullable: false),
                    IsGenerated = table.Column<bool>(nullable: false),
                    GeneratedDate = table.Column<DateTime>(nullable: false),
                    IsProcessing = table.Column<bool>(nullable: false),
                    ProcessingDate = table.Column<DateTime>(nullable: true),
                    IsError = table.Column<bool>(nullable: false),
                    Error = table.Column<string>(nullable: true),
                    UsedDate = table.Column<DateTime>(nullable: true),
                    CardNumber = table.Column<string>(nullable: true),
                    CardId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenItem_TokenBatch_TokenBatchId",
                        column: x => x.TokenBatchId,
                        principalTable: "TokenBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Index = table.Column<long>(nullable: false),
                    Signature = table.Column<string>(nullable: true),
                    TransactionDate = table.Column<DateTime>(nullable: false),
                    TransactionReference = table.Column<string>(nullable: true),
                    Identifier = table.Column<string>(nullable: true),
                    TransactionType = table.Column<int>(nullable: false),
                    WalletId = table.Column<Guid>(nullable: false),
                    Receipt = table.Column<byte[]>(nullable: true),
                    Validated = table.Column<bool>(nullable: false),
                    ValidatedDate = table.Column<DateTime>(nullable: true),
                    Generated = table.Column<bool>(nullable: false),
                    ErrorMessage = table.Column<string>(nullable: true),
                    ErrorType = table.Column<int>(nullable: false),
                    IsError = table.Column<bool>(nullable: false),
                    IsProcessing = table.Column<bool>(nullable: false),
                    StartProcessing = table.Column<DateTime>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transaction_Wallet_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PaymentType = table.Column<int>(nullable: false),
                    AmountCurrency = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    CurrencyCode = table.Column<string>(nullable: true),
                    CurrencyRate = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    PaymentReference = table.Column<string>(nullable: true),
                    IsValid = table.Column<bool>(nullable: false),
                    ValidatedDate = table.Column<DateTime>(nullable: true),
                    IsDeposit = table.Column<bool>(nullable: false),
                    DepositedDate = table.Column<DateTime>(nullable: true),
                    TransactionId = table.Column<Guid>(nullable: false),
                    IsStarting = table.Column<bool>(nullable: false),
                    StartingDate = table.Column<DateTime>(nullable: true),
                    IsValidated = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payment_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PurchaseId = table.Column<Guid>(nullable: true),
                    ItemType = table.Column<string>(nullable: true),
                    ItemId = table.Column<string>(nullable: true),
                    ItemReference = table.Column<string>(nullable: true),
                    Value = table.Column<int>(nullable: false),
                    Quantity = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseItem_Transaction_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Transaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionStatu",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TransactionStatuDate = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    TransactionStatus = table.Column<int>(nullable: false),
                    IsError = table.Column<bool>(nullable: false),
                    ErrorMessage = table.Column<string>(nullable: true),
                    TransactionId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionStatu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionStatu_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ad_AdPriceId",
                table: "Ad",
                column: "AdPriceId");

            migrationBuilder.CreateIndex(
                name: "IX_CardStatu_CardId",
                table: "CardStatu",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_TransactionId",
                table: "Payment",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItem_PurchaseId",
                table: "PurchaseItem",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenBatchStatu_TokenBatchId1",
                table: "TokenBatchStatu",
                column: "TokenBatchId1");

            migrationBuilder.CreateIndex(
                name: "IX_TokenItem_TokenBatchId",
                table: "TokenItem",
                column: "TokenBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_WalletId",
                table: "Transaction",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionStatu_TransactionId",
                table: "TransactionStatu",
                column: "TransactionId");
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
                name: "PurchaseItem");

            migrationBuilder.DropTable(
                name: "SocialNetwork");

            migrationBuilder.DropTable(
                name: "TokenBatchStatu");

            migrationBuilder.DropTable(
                name: "TokenItem");

            migrationBuilder.DropTable(
                name: "TransactionStatu");

            migrationBuilder.DropTable(
                name: "UrlShortned");

            migrationBuilder.DropTable(
                name: "AdPrice");

            migrationBuilder.DropTable(
                name: "Card");

            migrationBuilder.DropTable(
                name: "TokenBatch");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "Wallet");
        }
    }
}
