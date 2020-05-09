using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Location
{
    public class UpdateCountryDto : InsertCountryDto
    {
        [Required(ErrorMessage = "Id is required")]
        public int Id { get; set; }
    }
}