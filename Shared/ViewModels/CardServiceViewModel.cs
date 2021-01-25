using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abeer.Shared.Functional;

namespace Abeer.Shared.ViewModels
{
    public class CardServiceViewModel
    {
        public string PageName { get; set; }
        public string ImageUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool AuthenticationRequired {get;set;}
    } 
}
