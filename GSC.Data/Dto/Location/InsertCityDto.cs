using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Location
{
    public class InsertCityDto
    {
        [Required(ErrorMessage = "CityName is required")]
        public string CityName { get; set; }

        [Required(ErrorMessage = "CityCode is required")]
        public string CityCode { get; set; }

        [Required(ErrorMessage = "StateId is required")]
        public int StateId { get; set; }
    }
}