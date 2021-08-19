using GSC.Common.Base;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.SupplyManagement
{
    public class VerificationApprovalTemplateHistory : BaseEntity
    {
        public int VerificationApprovalTemplateId { get; set; }
        public int SendBy { get; set; }
        public DateTime SendOn { get; set; }
        public bool IsSendBack { get; set; }
        public VerificationApprovalTemplate VerificationApprovalTemplate { get; set; }
    }
}
