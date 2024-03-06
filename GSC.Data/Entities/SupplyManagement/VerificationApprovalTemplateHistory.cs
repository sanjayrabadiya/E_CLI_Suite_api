using GSC.Common.Base;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using System;
using System.ComponentModel.DataAnnotations.Schema;


namespace GSC.Data.Entities.SupplyManagement
{
    public class VerificationApprovalTemplateHistory : BaseEntity
    {
        public int VerificationApprovalTemplateId { get; set; }
        public int SecurityRoleId { get; set; }
        public int? SendBySecurityRoleId { get; set; }
        public int SendBy { get; set; }
        public DateTime SendOn { get; set; }
        public bool IsSendBack { get; set; }
        public int? AuditReasonId { get; set; }
        public AuditReason AuditReason { get; set; }
        public string ReasonOth { get; set; }
        public VerificationApprovalTemplate VerificationApprovalTemplate { get; set; }
        public ProductVerificationStatus? Status { get; set; }
        [ForeignKey("SendBy")]
        public User User { get; set; }
        public SecurityRole SecurityRole { get; set; }

        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}
