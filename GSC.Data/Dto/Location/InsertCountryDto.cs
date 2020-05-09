using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Location
{
    public class InsertCountryDto
    {
        [Required(ErrorMessage = "CountryName is required")]
        public string CountryName { get; set; }

        public string CountryCallingCode { get; set; }

        [Required(ErrorMessage = "CountryCode is required")]
        public string CountryCode { get; set; }
    }
}