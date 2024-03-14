using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Volunteer
{
    public class VolunteerFingerDto : BaseDto
    {
        public int VolunteerId { get; set; }
        public string FingerImage { get; set; }
    }

    public class VolunteerFingerAddDto
    {
        public int VolunteerId { get; set; }
        public string Template { get; set; }
    }
}