using System.ComponentModel.DataAnnotations;

namespace Abeer.Shared
{
    public class Country
    {
        [Key]
        public  long Id { get; set; }
        public string Name { get; set; }
        public string Gdp { get; set; }
        public string Faocode { get; set; }
        public string Label { get; set; }
        public string Eeacode { get; set; }
        public string Estatcode { get; set; }
        public string Nutscode { get; set; }
        public string Culture { get; set; }
    }
}
