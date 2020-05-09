using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Volunteer
{
    public class VolunteerFood : BaseEntity
    {
        public int VolunteerId { get; set; }

        public int FoodTypeId { get; set; }
    }
}