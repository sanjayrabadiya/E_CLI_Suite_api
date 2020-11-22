using GSC.Common.Base;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;
using GSC.Shared;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.Audit
{
    public class AuditTrailCommon 
    {
        [Key]
        public int Id { get; set; }
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string Action { get; set; }
        public string ColumnName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
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

        

        private DateTime? _createdDate;
        public DateTime? CreatedDate
        {
            get => _createdDate?.UtcDateTime();
            set => _createdDate = value?.UtcDateTime();
        }

        public int? CreatedBy { get; set; }


    }
}