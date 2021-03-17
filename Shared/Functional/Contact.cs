using Abeer.Shared.ReferentielTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Abeer.Shared
{
    public class ViewContact
    {
        public ViewContact()
        {

        }

        public ViewContact(ApplicationUser user, ApplicationUser contact)
        {
            if(user != null)
                Owner = user;

            if(contact != null)
                Contact = contact;
        }

        public ViewContact(ApplicationUser user, ApplicationUser uContact, Contact contact) : this(user, uContact)
        {
            if (contact != null)
            {
                Id = contact.Id;
                UserAccepted = contact.UserAccepted;
                UserId = contact.UserId;
                OwnerId = contact.OwnerId;
                DateAccepted = contact.DateAccepted;
            }
            else
            {
                Id = Guid.NewGuid();
            }
        }

        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string OwnerId { get; set; }
        public bool HasNewMsg { get; set; }
        public EnumUserAccepted UserAccepted { get; set; }
        public DateTime? DateAccepted { get; set; }
        public ViewApplicationUser Contact { get; set; }
        public ViewApplicationUser Owner { get; set; }
    }

    public class Contact
    {
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string OwnerId { get; set; }
        public EnumUserAccepted UserAccepted { get; set; }
        public DateTime? DateAccepted { get; set; }
    }
}
