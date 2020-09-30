
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Abeer.Shared
{
    public class Card
    {
        [Key]
        public Guid Id { get; set; }
        public string CardType { get; set; }
        public string CardNumber { get; set; }
        public string PinCode { get; set; }
        public int Value { get; set; }
        public bool IsGenerated { get; set; }
        public string CreatorId { get; set; }
        public byte[] CsvFileContent { get; set; }
        public List<CardStatu> CardStatus { get; set; }
        public bool IsUsed { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
        public ErrorTypes ErrorType { get; set; }
        public bool IsProcessing { get; set; }
        public DateTime? StartProcessing { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; }
        public string Icon { get; set; }
    }

    public enum ErrorTypes
    {
        ValueMissing,
        Application
    }

    public class CardStatu
    {
        [Key]
        public Guid Id { get; set; }
        public Guid CardId { get; set; }
        public Card Card { get; set; }
        public CardStatus Status { get; set; }
        public string StatusMessage { get; set; }
        public DateTime StatusDate { get; set; }
        public string UserId { get; set; }
    }


    public enum CardStatus : short
    {
        Created=0, Started=1, Finished=2, Error=3,
        Generated = 4,
        Inserted = 5,
        Updated = 6
    }
}
