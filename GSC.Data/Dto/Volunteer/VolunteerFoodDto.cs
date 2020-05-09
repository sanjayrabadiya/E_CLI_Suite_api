using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Volunteer
{
    public class VolunteerFoodDto : BaseDto
    {
        public int VolunteerId { get; set; }

        [Required(ErrorMessage = "Food Type is required.")]
        public int FoodTypeId { get; set; }

        public string FoodTypeName { get; set; }

        public bool Selected { get; set; }

        public List<int> FoodTypeIds { get; set; }
    }
}