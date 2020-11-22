using GSC.Common.Base;

namespace GSC.Data.Entities.Volunteer
{
    public class VolunteerHistory : BaseEntity
    {
        public int VolunteerId { get; set; }

        public string Note { get; set; }
    }
}