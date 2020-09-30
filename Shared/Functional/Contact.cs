using System.ComponentModel.DataAnnotations;

namespace Abeer.Shared
{
    public class Contact
    {
        [Key]
        public long Id { get; set; }
        public string OwnerId { get; set; }
        public string DisplayName { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FacebookUrl { get; set; }
        public string WhatsAppUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string LinkedInUrl { get; set; }
        public string SkypeUrl { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
    }
}
