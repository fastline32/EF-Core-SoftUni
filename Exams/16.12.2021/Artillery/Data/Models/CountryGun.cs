using System.ComponentModel.DataAnnotations.Schema;
using Castle.DynamicProxy.Generators.Emitters;

namespace Artillery.Data.Models
{
    public class CountryGun
    {
        [ForeignKey("Country")]
        public int CountryId { get; set; }
        public Country Country { get; set; }
        [ForeignKey("Gun")]
        public int GunId { get; set; }
        public Gun Gun { get; set; }
    }
}

//•	CountryId – Primary Key integer, foreign key (required)
//    •	GunId – Primary Key integer, foreign key (required)
