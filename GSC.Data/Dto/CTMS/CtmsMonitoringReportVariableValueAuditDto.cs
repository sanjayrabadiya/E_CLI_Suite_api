using GSC.Data.Entities.Common;
using GSC.Helper;
using System;

namespace GSC.Data.Dto.CTMS
{
    public class CtmsMonitoringReportVariableValueAuditDto : BaseDto
    {
        public int CtmsMonitoringReportVariableValueId { get; set; }
        public string Value { get; set; }
        public string Note { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? CreatedDate{ get; set; }
        public string OldValue { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        public string Reason { get; set; }
        public string ReasonOth { get; set; }
        public string NewValue { get; set; }
        public string Role { get; set; }
        public string User { get; set; }
        public CollectionSources CollectionSource { get; set; }
    }
}
