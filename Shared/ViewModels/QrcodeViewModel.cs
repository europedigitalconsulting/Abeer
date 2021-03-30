using System;

namespace Abeer.Shared.ViewModels
{
    public class QrcodeViewModel
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string Key { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
