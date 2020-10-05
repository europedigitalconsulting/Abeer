using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AbeerContactShared.Data.Repo
{
    public class Annonce
    {
        public int ID { get; set; }
        public string IdUser { get; set; }
        public string TitleAnnonce { get; set; }
        public string DescriptionAnnonce { get; set; }
        public string PathImageAnnonce { get; set; }

        public DateTime DatePublicationAnnonce { get; set; }

        public bool isFree { get; set; }

    }
}
