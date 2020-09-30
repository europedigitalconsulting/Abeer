namespace Abeer.Shared.ViewModels
{
    public class InvoiceRowViewModel
    {
        public InvoiceRowViewModel(PurchaseItem pi)
        {
            Description = pi.ItemType + " réf : " + pi.ItemReference + "(" + pi.Value + ")";
            Quantity = pi.Quantity;
            Value = pi.Value;
        }

        public string Description { get; set; }
        public int Quantity { get; set; }
        public int Value { get; set; }
    }
}