using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abeer.Shared
{

    public class Purchase : Transaction
    {
        public List<PurchaseItem> PurchaseItems { get; set; }
    }

    public class PurchaseItem
    {
        [Key]
        public Guid Id { get; set; }
        public Purchase Purchase { get; set; }
        public string ItemType { get; set; }
        public string ItemId { get; set; }
        public string ItemReference { get; set; }
        public int Value { get; set; }
        public int Quantity { get; set; }
    }

    public class Payment
    {
        [Key]
        public Guid Id { get; set; }
        public PaymentTypes PaymentType { get; set; }
        [Column(TypeName = "decimal(18,5)")]
        public decimal AmountCurrency { get; set; }
        public string CurrencyCode { get; set; }
        [Column(TypeName = "decimal(18,5)")]
        public decimal CurrencyRate { get; set; }
        public string PaymentReference { get; set; }
        public bool IsValid { get; set; }
        public DateTime? ValidatedDate { get; set; }
        public bool IsDeposit { get; set; }
        public DateTime? DepositedDate { get; set; }
        public Guid TransactionId { get; set; }
        public Transaction Transaction { get; set; }
        public bool IsStarting { get; set; }
        public DateTime? StartingDate { get; set; }
        public bool IsValidated { get; set; }
    }

    public enum PaymentTypes
    {
        BankCheck, BankTransfert, Card, Coins, Virtual
    }

    public abstract class Transaction
    {
        [Key]
        public Guid Id { get; set; }
        public long Index { get; set; }
        public string Signature { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionReference { get; set; }
        public string Identifier { get; set; }
        public TransactionType TransactionType { get; set; }
        public Guid WalletId { get; set; }
        public Wallet Wallet { get; set; }
        public byte[] Receipt { get; set; }
        public List<TransactionStatu> TransactionStatus { get; set; }
        public List<Payment> Payments { get; set; }
        public bool Validated { get; set; }
        public DateTime? ValidatedDate { get; set; }
        public bool Generated { get; set; }
        public string ErrorMessage { get; set; }
        public ErrorTypes ErrorType { get; set; }
        public bool IsError { get; set; }
        public bool IsProcessing { get; set; }
        public DateTime? StartProcessing { get; set; }

    }

    public enum TransactionType
    {
        Input=0, Output=1
    }

    public  class TransactionStatu
    {
        [Key]
        public  Guid Id { get; set; }
        public DateTime TransactionStatuDate { get; set; }
        public string UserId { get; set; }
        public TransactionStatus TransactionStatus { get; set; }
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
        public Transaction Transaction { get; set; }
    }

    public enum TransactionStatus
    {
        Created=0, Doing=1, Done,
        Generated
    }
}