using System;
using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningHistory : BaseEntity, ICommonAduit
    {
        public int ScreeningEntryId { get; set; }

        public DateTime? XrayDate { get; set; }

        public DateTime? NextXrayDueDate { get; set; }

        public DateTime? LastPkSampleDate { get; set; }

        public DateTime? NextEligibleDate { get; set; }

        public bool? Enrolled { get; set; }
        public bool? IsCompleted { get; set; }
        public string ProjectNumber { get; set; }
        public string Reason { get; set; }
        public ScreeningEntry ScreeningEntry { get; set; }
    }
}