using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Audit;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Volunteer
{
    public class VolunteerContactDto : BaseDto
    {
        public int VolunteerId { get; set; }

        [Required(ErrorMessage = "Contact Type is required.")]
        public int ContactTypeId { get; set; }

        [Required(ErrorMessage = "Contact Number is required.")]
        public string ContactNo { get; set; }

        public string ContactName { get; set; }

        public bool IsDefault { get; set; }

        public bool IsEmergency { get; set; }

        public string ContactTypeName { get; set; }

        public List<AuditTrail> Changes { get; set; }
    }
}