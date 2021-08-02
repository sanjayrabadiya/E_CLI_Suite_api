using GSC.Data.Entities.Common;
using System;

namespace GSC.Data.Dto.Audit
{
    public class AuditTrailDto : BaseDto
    {
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string Action { get; set; }
        public string ColumnName { get; set; }
        public string LabelName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public bool? IsRecordDeleted { get; set; }
        public string ReasonOth { get; set; }
        public int UserId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ReasonName { get; set; }
        public string UserName { get; set; }
        public string UserRoleName { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }

    public class ProjectDesignAuditReportDto : BaseDto
    {
        public int Key { get; set; }
        public string StudyCode { get; set; }
        public string Period { get; set; }
        public string Visit { get; set; }
        public string Template { get; set; }
        public string Variable { get; set; }
        public string IpAddress { get; set; }
        public string Action { get; set; }
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Reason { get; set; }
        public string Comment { get; set; }
        public string User { get; set; }
        public string Role { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string TimeZone { get; set; }
        public string DeleteByUser { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}
