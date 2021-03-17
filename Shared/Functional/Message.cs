using System;
using System.ComponentModel.DataAnnotations;

namespace Abeer.Shared
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserIdFrom { get; set; }
        public Guid UserIdTo { get; set; }
        public string Text { get; set; }
        public DateTime DateSent { get; set; }
        public DateTime? DateReceived { get; set; }
    }
}
