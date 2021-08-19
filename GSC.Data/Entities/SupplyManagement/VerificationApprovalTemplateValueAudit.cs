using GSC.Data.Entities.Master;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.SupplyManagement
{
    public class VerificationApprovalTemplateValueAudit
    {
        [Key]
        public int Id { get; set; }
        public int VerificationApprovalTemplateValueId { get; set; }

        public string Value { get; set; }
        public string Note { get; set; }
        public int? ReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string OldValue { get; set; }
        public string IpAddress { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public string TimeZone { get; set; }
        public DateTime? CreatedDate{get;set;}
        public VerificationApprovalTemplateValue VerificationApprovalTemplateValue { get; set; }
        [ForeignKey("ReasonId")] public AuditReason AuditReason { get; set; }
    }
}
