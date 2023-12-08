using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.SupplyManagement
{
    public class VerificationApprovalTemplateHistoryDto : BaseDto
    {
        public int VerificationApprovalTemplateId { get; set; }
        public int SendBy { get; set; }
        public DateTime SendOn { get; set; }
        public bool IsSendBack { get; set; }
        public int? AuditReasonId { get; set; }
        public AuditReason AuditReason { get; set; }
        public string ReasonOth { get; set; }
        public int SecurityRoleId { get; set; }
        //public SecurityRole SecurityRole { get; set; }
        public int? SendBySecurityRoleId { get; set; }
    }

    public class VerificationApprovalTemplateHistoryViewDto : BaseAuditDto
    {
        public string SendBy { get; set; }
        public DateTime SendOn { get; set; }
        public string Role { get; set; }
        public string AuditReason { get; set; }
        public string ReasonOth { get; set; }
        public string Status { get; set; }

        public int? SendBySecurityRoleId { get; set; }
        public string SendByRole { get; set; }

        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}
