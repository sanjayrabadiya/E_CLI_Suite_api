using System;
using System.Collections.Generic;
using GSC.Data.Entities.Audit;
using GSC.Data.Entities.Common;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningHistoryDto : BaseDto
    {
        public int ScreeningEntryId { get; set; }
        public DateTime? XrayDate { get; set; }
        public DateTime? NextXrayDueDate { get; set; }
        public DateTime? LastPkSampleDate { get; set; }
        public DateTime? NextEligibleDate { get; set; }
        public bool? Enrolled { get; set; }
        public bool? IsFitnessFit { get; set; }
        public bool? IsCompleted { get; set; }
        public string ProjectNumber { get; set; }
        public string Reason { get; set; }
        public DateTime ScreeningDate { get; set; }
        public string VolunteerNumber { get; set; }
        public string VolunteerName { get; set; }
        public int VolunteerId { get; set; }
        public string ScreeningStatus { get; set; }
        public string Notes { get; set; }
        public List<VolunteerAuditTrail> Changes { get; set; }
    }
}