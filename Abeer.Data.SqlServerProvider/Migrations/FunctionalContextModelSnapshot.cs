﻿// <auto-generated />
using System;
using Abeer.Data.SqlServerProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Abeer.Data.SqlServerProvider.Migrations
{
    [DbContext(typeof(FunctionalContext))]
    partial class FunctionalContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Abeer.Data.Models.UrlShortned", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("ClickedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Code")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsClicked")
                        .HasColumnType("bit");

                    b.Property<bool>("IsSecure")
                        .HasColumnType("bit");

                    b.Property<bool>("IsSingle")
                        .HasColumnType("bit");

                    b.Property<string>("LongUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SecureKey")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ShortUrl")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("UrlShortned");
                });

            modelBuilder.Entity("Abeer.Shared.Card", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CardNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CardType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CreatorId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("CsvFileContent")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ErrorType")
                        .HasColumnType("int");

                    b.Property<string>("GeneratedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("GeneratedDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("HasError")
                        .HasColumnType("bit");

                    b.Property<string>("Icon")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsGenerated")
                        .HasColumnType("bit");

                    b.Property<bool>("IsProcessing")
                        .HasColumnType("bit");

                    b.Property<bool>("IsSold")
                        .HasColumnType("bit");

                    b.Property<bool>("IsUsed")
                        .HasColumnType("bit");

                    b.Property<string>("PinCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<string>("SoldBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("SoldDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("StartProcessing")
                        .HasColumnType("datetime2");

                    b.Property<int>("Value")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Card");
                });

            modelBuilder.Entity("Abeer.Shared.CardStatu", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CardId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<short>("Status")
                        .HasColumnType("smallint");

                    b.Property<DateTime>("StatusDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("StatusMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CardId");

                    b.ToTable("CardStatu");
                });

            modelBuilder.Entity("Abeer.Shared.Contact", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("OwnerId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Contact");
                });

            modelBuilder.Entity("Abeer.Shared.Country", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Culture")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Eeacode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Estatcode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Faocode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Gdp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Label")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Nutscode")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Country");
                });

            modelBuilder.Entity("Abeer.Shared.CustomLink", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BackgroundColor")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DisplayInfo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Logo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OwnerId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Url")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("CustomLink");
                });

            modelBuilder.Entity("Abeer.Shared.Functional.AdModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AdPriceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("EndDisplayTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("ImageUrl1")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImageUrl2")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImageUrl3")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImageUrl4")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsValid")
                        .HasColumnType("bit");

                    b.Property<string>("OwnerId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PaymentInformation")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("StartDisplayTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Url1")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Url2")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Url3")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Url4")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ValidateDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("ViewCount")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AdPriceId");

                    b.ToTable("Ad");
                });

            modelBuilder.Entity("Abeer.Shared.Functional.AdPrice", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("DelayToDisplay")
                        .HasColumnType("int");

                    b.Property<int?>("DisplayDuration")
                        .HasColumnType("int");

                    b.Property<int>("MaxViewCount")
                        .HasColumnType("int");

                    b.Property<string>("PriceDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PriceName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Value")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.ToTable("AdPrice");
                });

            modelBuilder.Entity("Abeer.Shared.Payment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("AmountCurrency")
                        .HasColumnType("decimal(18,5)");

                    b.Property<string>("CurrencyCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("CurrencyRate")
                        .HasColumnType("decimal(18,5)");

                    b.Property<DateTime?>("DepositedDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeposit")
                        .HasColumnType("bit");

                    b.Property<bool>("IsStarting")
                        .HasColumnType("bit");

                    b.Property<bool>("IsValid")
                        .HasColumnType("bit");

                    b.Property<bool>("IsValidated")
                        .HasColumnType("bit");

                    b.Property<string>("PaymentReference")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PaymentType")
                        .HasColumnType("int");

                    b.Property<DateTime?>("StartingDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("TransactionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("ValidatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("TransactionId");

                    b.ToTable("Payment");
                });

            modelBuilder.Entity("Abeer.Shared.PurchaseItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ItemId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ItemReference")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ItemType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("PurchaseId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<int>("Value")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PurchaseId");

                    b.ToTable("PurchaseItem");
                });

            modelBuilder.Entity("Abeer.Shared.SocialNetwork", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BackgroundColor")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DisplayInfo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Logo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OwnerId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Url")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("SocialNetwork");
                });

            modelBuilder.Entity("Abeer.Shared.TokenBatch", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("BatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("CsvFileContent")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ErrorType")
                        .HasColumnType("int");

                    b.Property<DateTime>("GeneratedDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsError")
                        .HasColumnType("bit");

                    b.Property<bool>("IsGenerated")
                        .HasColumnType("bit");

                    b.Property<string>("OperatorId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OperatorName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PartsItemsCount")
                        .HasColumnType("int");

                    b.Property<string>("TokenType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WebHookAuthentication")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WebHookProtocolType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("WebHookUrl")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("TokenBatch");
                });

            modelBuilder.Entity("Abeer.Shared.TokenBatchStatu", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<short>("Status")
                        .HasColumnType("smallint");

                    b.Property<DateTime>("StatusDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("StatusMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("TokenBatchId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long?>("TokenBatchId1")
                        .HasColumnType("bigint");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("TokenBatchId1");

                    b.ToTable("TokenBatchStatu");
                });

            modelBuilder.Entity("Abeer.Shared.TokenItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("CardId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CardNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Error")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("GeneratedDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsError")
                        .HasColumnType("bit");

                    b.Property<bool>("IsGenerated")
                        .HasColumnType("bit");

                    b.Property<bool>("IsProcessing")
                        .HasColumnType("bit");

                    b.Property<bool>("IsUsed")
                        .HasColumnType("bit");

                    b.Property<string>("PartNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("PartPosition")
                        .HasColumnType("bigint");

                    b.Property<string>("PinCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ProcessingDate")
                        .HasColumnType("datetime2");

                    b.Property<long>("TokenBatchId")
                        .HasColumnType("bigint");

                    b.Property<string>("UsedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UsedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("TokenBatchId");

                    b.ToTable("TokenItem");
                });

            modelBuilder.Entity("Abeer.Shared.Transaction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ErrorType")
                        .HasColumnType("int");

                    b.Property<bool>("Generated")
                        .HasColumnType("bit");

                    b.Property<string>("Identifier")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Index")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsError")
                        .HasColumnType("bit");

                    b.Property<bool>("IsProcessing")
                        .HasColumnType("bit");

                    b.Property<byte[]>("Receipt")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("Signature")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("StartProcessing")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("TransactionDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("TransactionReference")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TransactionType")
                        .HasColumnType("int");

                    b.Property<bool>("Validated")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("ValidatedDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("WalletId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("WalletId");

                    b.ToTable("Transaction");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Transaction");
                });

            modelBuilder.Entity("Abeer.Shared.TransactionStatu", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsError")
                        .HasColumnType("bit");

                    b.Property<Guid?>("TransactionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("TransactionStatuDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("TransactionStatus")
                        .HasColumnType("int");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("TransactionId");

                    b.ToTable("TransactionStatu");
                });

            modelBuilder.Entity("Abeer.Shared.Wallet", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Identifier")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OwnerId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Wallet");
                });

            modelBuilder.Entity("Abeer.Shared.Purchase", b =>
                {
                    b.HasBaseType("Abeer.Shared.Transaction");

                    b.HasDiscriminator().HasValue("Purchase");
                });

            modelBuilder.Entity("Abeer.Shared.CardStatu", b =>
                {
                    b.HasOne("Abeer.Shared.Card", "Card")
                        .WithMany("CardStatus")
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Abeer.Shared.Functional.AdModel", b =>
                {
                    b.HasOne("Abeer.Shared.Functional.AdPrice", "AdPrice")
                        .WithMany()
                        .HasForeignKey("AdPriceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Abeer.Shared.Payment", b =>
                {
                    b.HasOne("Abeer.Shared.Transaction", "Transaction")
                        .WithMany("Payments")
                        .HasForeignKey("TransactionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Abeer.Shared.PurchaseItem", b =>
                {
                    b.HasOne("Abeer.Shared.Purchase", "Purchase")
                        .WithMany("PurchaseItems")
                        .HasForeignKey("PurchaseId");
                });

            modelBuilder.Entity("Abeer.Shared.TokenBatchStatu", b =>
                {
                    b.HasOne("Abeer.Shared.TokenBatch", "TokenBatch")
                        .WithMany("TokenBatchStatus")
                        .HasForeignKey("TokenBatchId1");
                });

            modelBuilder.Entity("Abeer.Shared.TokenItem", b =>
                {
                    b.HasOne("Abeer.Shared.TokenBatch", "TokenBatch")
                        .WithMany("TokenItems")
                        .HasForeignKey("TokenBatchId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Abeer.Shared.Transaction", b =>
                {
                    b.HasOne("Abeer.Shared.Wallet", "Wallet")
                        .WithMany("Transactions")
                        .HasForeignKey("WalletId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Abeer.Shared.TransactionStatu", b =>
                {
                    b.HasOne("Abeer.Shared.Transaction", "Transaction")
                        .WithMany("TransactionStatus")
                        .HasForeignKey("TransactionId");
                });
#pragma warning restore 612, 618
        }
    }
}