using GSC.Common.Base;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using GSC.Shared.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.Audit
{
    public class AuditTrail : BaseEntity
    {
        public AuditModule ModuleId { get; set; }
        public AuditTable TableId { get; set; }
        public int RecordId { get; set; }
        public int? ParentRecordId { get; set; }
        public AuditAction Action { get; set; }
        public string ColumnName { get; set; }
        public string LabelName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public bool? IsRecordDeleted { get; set; }
        public int? ReasonId { get; set; }
        public AuditReason Reason { get; set; }
        public string ReasonOth { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        public int UserRoleId { get; set; }
        public int? CompanyId { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}