using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Location
{
    public class InsertStateDto
    {
        [Required(ErrorMessage = "StateName is required")]
        public string StateName { get; set; }

        [Required(ErrorMessage = "CountryId is required")]
        public int CountryId { get; set; }
    }
}