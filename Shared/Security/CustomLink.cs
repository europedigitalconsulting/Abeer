using System;
using System.ComponentModel.DataAnnotations;

namespace Abeer.Shared
{
    public class CustomLink
    {
        public CustomLink()
        {
            Id = Guid.NewGuid().ToString();
        }

        [Key]
        public string Id { get; set; }
        public string OwnerId { get; set; }
        public string BackgroundColor { get; set; }
        public string Name { get; set; }
        public string DisplayInfo { get; set; }
        public string Logo { get; set; }
        public string Url { get; set; }       
    }
}