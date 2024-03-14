using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;

namespace GSC.Data.Entities.Volunteer
{
    public class VolunteerContact : BaseEntity, ICommonAduit
    {
        public int VolunteerId { get; set; }

        public int? ContactTypeId { get; set; }
        public ContactType ContactType { get; set; }

        public string ContactNo { get; set; }
        public string ContactNoTwo { get; set; }

        public string ContactName { get; set; }

        public bool IsDefault { get; set; }

        public bool IsEmergency { get; set; }
    }
}