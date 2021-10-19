using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.CTMS
{
    public class ManageMonitoringReportVariableAuditDto : BaseDto
    {
        public int ManageMonitoringReportVariableId { get; set; }
        public string Value { get; set; }
        public string Note { get; set; }
        public int? ReasonId { get; set; }
        public string ReasonName { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? CreatedDate{ get; set; }
        public string OldValue { get; set; }
    }
}
