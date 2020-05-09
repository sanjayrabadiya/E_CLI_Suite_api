using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Location
{
    public class LocationDto : BaseDto
    {
        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

        public int? CountryId { get; set; }

        public int? StateId { get; set; }

        public int? CityId { get; set; }

        public string Zip { get; set; }
    }
}