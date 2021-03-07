using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.Shared.Functional
{
    public class Invitation
    {
        public Invitation()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.UtcNow;
        }

        [Key]
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string OwnedId { get; set; }
        public string ContactId { get; set; }
        public int InvitationStat { get; set; }

    }
}
