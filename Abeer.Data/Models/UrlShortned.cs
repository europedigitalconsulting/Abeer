
using System;
using System.ComponentModel.DataAnnotations;

namespace Abeer.Data.Models
{
    public class UrlShortned
    {
        [Key]
        public long Id { get; set; }
        public string Code { get; set; }
        public bool IsSingle { get; set; }
        public bool IsSecure { get; set; }
        public string SecureKey { get; set; }
        public bool IsClicked { get; set; }
        public DateTime? ClickedDate { get; set; }
        public string LongUrl { get; set; }
        public string ShortUrl { get; set; }
    }
}
