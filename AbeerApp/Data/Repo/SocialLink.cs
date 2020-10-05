using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AbeerContactShared.Data.Repo
{
    public class SocialLink
    {
        public int ID { get; set; }
        public string IdUser { get; set; }
        public string UrlSocialProfile { get; set; }
        public string TypeSocialLink { get; set; }
    }
}
