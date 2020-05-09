using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Volunteer
{
    public class VolunteerHistory : BaseEntity
    {
        public int VolunteerId { get; set; }

        public string Note { get; set; }
    }
}