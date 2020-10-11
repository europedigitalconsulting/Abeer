using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Abeer.Shared
{
    public class SocialNetwork
    {
        [Key]
        public string Id { get; set; }

        public SocialNetwork()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string OwnerId { get; set; }

        public string BackgroundColor { get; set; }
        public string Name { get; set; }
        public string DisplayInfo { get; set; }
        public string Logo { get; set; }
        public string Url { get; set; }

    //    public Dictionary<string,string> Datas { get; set; } = new Dictionary<string, string>();
       
    }
}