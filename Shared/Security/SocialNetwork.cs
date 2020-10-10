using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Abeer.Shared
{
    public class SocialNetwork
    {
        public SocialNetwork()
        {
            Id = Guid.NewGuid().ToString();
        }


        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string DisplayInfo { get; set; }
        public string Logo { get; set; }
        public string Url { get; set; }

    //    public Dictionary<string,string> Datas { get; set; } = new Dictionary<string, string>();
       
    }
}