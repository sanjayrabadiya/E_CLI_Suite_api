using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;

namespace GSC.Data.Entities.CTMS
{
    public class ManageMonitoringReportVariableComment : BaseEntity
    {
        public int ManageMonitoringReportVariableId { get; set; }
        public int? RoleId { get; set; }
        public string Comment { get; set; }
        public ManageMonitoringReportVariable ManageMonitoringReportVariable { get; set; }
        public int? ReasonId { get; set; }
        [ForeignKey("ReasonId")] public AuditReason Reason { get; set; }
        public string ReasonOth { get; set; }
        public string TimeZone { get; set; }
        public CtmsCommentStatus? CommentStatus { get; set; }
        [ForeignKey("RoleId")] public SecurityRole Role { get; set; }
        public string Note { get; set; }
        public string Value { get; set; }
        public string OldValue { get; set; }
    }
}