using System;

namespace Abeer.Ads.Shared
{
    public class CategoryAdViewModel
    {
        public Guid Id { get; set; }
        public Guid AdId { get; set; }
        public Guid CategoryId { get; set; }
        public DateTime CreateDate { get; set; }
    }
}