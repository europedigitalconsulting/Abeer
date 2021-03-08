using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Abeer.Shared.Functional
{
    public class SubscriptionPack
    {
        [Key]
        public Guid Id { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool Enable { get; set; }
        public int Duration{ get; set; }
        public bool Popuplar { get; set; }
    }
}
