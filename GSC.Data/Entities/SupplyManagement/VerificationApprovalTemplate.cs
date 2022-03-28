using GSC.Common.Base;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Data.Entities.UserMgt;
using System;

namespace GSC.Data.Entities.SupplyManagement
{
    public class VerificationApprovalTemplate:BaseEntity
    {
        public int StudyLevelFormId { get; set; }
        public int ProductVerificationDetailId { get; set; }
        public int SecurityRoleId { get; set; }

        public DateTime? ApproveOn { get; set; }
        public bool IsApprove { get; set; }
        public StudyLevelForm StudyLevelForm { get; set; }
        public ProductVerificationDetail ProductVerificationDetail { get; set; }
        public VerificationApprovalTemplateHistory VerificationApprovalTemplateHistory { get; set; } = null;
        public SecurityRole SecurityRole { get; set; }

    }
}
