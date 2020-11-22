using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;

namespace GSC.Data.Entities.Location
{
    public class Location : BaseEntity
    {
        public string Address { get; set; }
        public int? CountryId { get; set; }
        public int? StateId { get; set; }
        public int? CityId { get; set; }
        public int? CityAreaId { get; set; }
        public string Zip { get; set; }
        public Country Country { get; set; }
        public State State { get; set; }
        public City City { get; set; }
        [NotMapped] public string CityAreaName { get; set; }

        [NotMapped] public string CityName { get; set; }

        [NotMapped] public string StateName { get; set; }

        [NotMapped] public string CountryName { get; set; }

        [NotMapped]
        public string FullAddress => Address + " " + CityAreaName + " " + CityName + " " + Zip + " " + StateName + " " +
                                     CountryName;
    }
}