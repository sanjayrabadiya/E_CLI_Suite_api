using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;

namespace GSC.Data.Dto.Location
{
    public class CityAreaDto : BaseDto
    {
        [Required(ErrorMessage = "Area Name is required.")]
        public string AreaName { get; set; }
        [Required(ErrorMessage = "City is required.")]
        public int? CityId { get; set; }
        public City City { get; set; }
        public int? StateId { get; set; }
        public int? CountryId { get; set; }
        public int? CompanyId { get; set; }
    }

    public class CityAreaGridDto : BaseAuditDto
    {
        public string AreaName { get; set; }
        public int? CityId { get; set; }
        public string CityName { get; set; }
        public City City { get; set; }
        public int? StateId { get; set; }
        public int? CountryId { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
    }
}