using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;

namespace GSC.Data.Dto.Location
{
    public class CityDto : BaseDto
    {
        [Required(ErrorMessage = "City Code is required.")]
        public string CityCode { get; set; }

        [Required(ErrorMessage = "City Name is required.")]
        public string CityName { get; set; }

        public int StateId { get; set; }

        public int CountryId { get; set; }

        public string StateName { get; set; }

        public string CountryName { get; set; }

        public State State { get; set; }

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int? CreatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
    }
}