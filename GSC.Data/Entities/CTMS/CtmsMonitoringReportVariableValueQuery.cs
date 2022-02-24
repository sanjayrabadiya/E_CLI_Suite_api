using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
    public class CtmsMonitoringReportVariableValueQuery : BaseEntity, ICommonAduit
    {
        public int CtmsMonitoringReportVariableValueId { get; set; }
        public string Value { get; set; }
        public int ReasonId { get; set; }
        public string ReasonOth { get; set; }
        public CtmsCommentStatus? QueryStatus { get; set; }
        public short QueryLevel { get; set; }
        public string Note { get; set; }
        public string OldValue { get; set; }
        public bool IsSystem { get; set; }
        public string UserName { get; set; }    
        public string UserRole { get; set; }
        public string TimeZone { get; set; }
        public int? QueryParentId { get; set; }
        public DateTime? PreviousQueryDate { get; set; }
        public CtmsMonitoringReportVariableValue CtmsMonitoringReportVariableValue { get; set; }
        public StudyLevelFormVariable StudyLevelFormVariable { get; set; }
        [ForeignKey("ReasonId")] public AuditReason Reason { get; set; }
    }
}
