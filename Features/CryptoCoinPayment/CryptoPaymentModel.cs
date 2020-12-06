using System;
using System.Collections.Generic;
using System.Text;

namespace Cryptocoin.Payment
{
    public class CryptoPaymentModel
    {
        public string Id{ get; set; }
        public string ClientId{ get; set; }
        public string ClientSecret { get; set; }
        public string RedirectSuccessServer { get; set; }
        public string RedirectErrorServer { get; set; }
        public string RedirectSuccess { get; set; }
         public string RedirectError { get; set; } 
         public string DomainApiPayment { get; set; } 
        public decimal Price { get; set; }
        public string OrderNumber { get; set; }
        public List<Tuple<string, decimal>> Items { get; set; }
    }
}
