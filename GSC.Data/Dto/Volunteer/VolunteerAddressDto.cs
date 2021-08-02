using System;
using System.Collections.Generic;
using GSC.Data.Entities.Audit;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Volunteer
{
    public class VolunteerAddressDto : BaseDto
    {
        public int VolunteerId { get; set; }

        public bool IsCurrent { get; set; }

        public bool IsPermanent { get; set; }

        public Entities.Location.Location Location { get; set; }

        public List<VolunteerAuditTrail> Changes { get; set; }
    }
}