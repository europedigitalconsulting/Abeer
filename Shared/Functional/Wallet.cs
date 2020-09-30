using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Abeer.Shared
{
    public class Wallet
    {
        [Key]
        public Guid Id { get; set; }
        public string OwnerId { get; set; }
        public List<Transaction> Transactions { get; set; }
        public string Identifier { get; set; }
    }
}
