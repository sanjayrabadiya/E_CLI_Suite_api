using GSC.Data.Entities.Common;
using System;

namespace GSC.Data.Dto.Audit
{
    public class AuditTrailCommonDto : BaseDto
    {
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string Action { get; set; }
        public string ColumnName { get; set; }
        public string LabelName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public bool? IsRecordDeleted { get; set; }
        public int? ReasonId { get; set; }
        public string ReasonOth { get; set; }
        public int UserId { get; set; }
        public int UserRoleId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ReasonName { get; set; }
        public string UserName { get; set; }
        public string UserRoleName { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}
