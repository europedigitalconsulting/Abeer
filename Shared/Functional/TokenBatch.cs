using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Abeer.Shared
{
    public class TokenBatch
    {
        [Key]
        public long Id { get; set; }
        public string TokenType { get; set; }
        public string BatchNumber { get; set; }
        public int PartsItemsCount { get; set; }
        public string OperatorId { get; set; }
        public string OperatorName { get; set; }
        public bool IsGenerated { get; set; }
        public byte[] CsvFileContent { get; set; }
        public virtual List<TokenItem> TokenItems { get; set; }
        public List<TokenBatchStatu> TokenBatchStatus { get; set; }
        public bool IsError { get; set; }
        public ErrorTypes ErrorType { get; set; }
        public string ErrorMessage { get; set; }
        public string WebHookUrl { get; set; }
        public string WebHookAuthentication { get; set; }
        public string WebHookProtocolType { get; set; }
        public DateTime GeneratedDate { get; set; }
    }

    public class TokenBatchStatu
    {
        public Guid Id { get; set; }
        public Guid TokenBatchId { get; set; }
        public TokenBatch TokenBatch { get; set; }
        public TokenBatchStatus Status { get; set; }
        public string StatusMessage { get; set; }
        public DateTime StatusDate { get; set; }
        public string UserId { get; set; }
    }

    public enum TokenBatchStatus : short
    {
        Created=0, Started=1, Finished=2, Error=3,
        Updated = 4,
        Generated = 5
    }

    public class TokenItem
    {
        [Key]
        public Guid Id { get; set; }
        public string PartNumber { get; set; }
        public bool IsUsed { get; set; }
        public string UsedBy { get; set; }
        public long PartPosition { get; set; }
        public string PinCode { get; set; }
        public long TokenBatchId { get; set; }
        public TokenBatch TokenBatch { get; set; }
        public bool IsGenerated { get; set; }
        public DateTime GeneratedDate { get; set; }
        public bool IsProcessing { get; set; }
        public DateTime? ProcessingDate { get; set; }
        public bool IsError { get; set; }
        public string Error { get; set; }
        public DateTime? UsedDate { get; set; }
        public string CardNumber { get; set; }
        public Guid? CardId { get; set; }
    }
}
