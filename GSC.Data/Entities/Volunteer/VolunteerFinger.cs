using GSC.Common.Base;

namespace GSC.Data.Entities.Volunteer
{
    public class VolunteerFinger : BaseEntity
    {
        public int VolunteerId { get; set; }
        public string FingerImage { get; set; }
        public Volunteer Volunteer { get; set; }
    }
}