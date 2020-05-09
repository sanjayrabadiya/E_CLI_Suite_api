using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Volunteer
{
    public class VolunteerHistoryDto : BaseDto
    {
        public int VolunteerId { get; set; }

        [Required(ErrorMessage = "Note is required.")]
        public string Note { get; set; }
    }
}