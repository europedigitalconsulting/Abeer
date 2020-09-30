using System;
using System.Collections.Generic;
using System.Text;

namespace Abeer.Shared
{
    public class PurchaseableCardViewModel
    {
        public string CardType { get; set; }
        public int InStock { get; set; }
        public string Icon { get; set; }
        public decimal Value { get; set; } 
        public int QuantityToPurchase { get; set; }
    }
}
