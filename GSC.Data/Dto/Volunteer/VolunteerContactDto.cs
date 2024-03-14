using System.Collections.Generic;
using GSC.Data.Entities.Audit;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Volunteer
{
    public class VolunteerContactDto : BaseDto
    {
        public int VolunteerId { get; set; }
        public int? ContactTypeId { get; set; }

        public string ContactNo { get; set; }
        public string ContactNoTwo { get; set; }

        public string ContactName { get; set; }

        public bool IsDefault { get; set; }

        public bool IsEmergency { get; set; }

        public string ContactTypeName { get; set; }

        public List<VolunteerAuditTrail> Changes { get; set; }
    }
}