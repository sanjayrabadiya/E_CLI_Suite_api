using GSC.Common.Base;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.SupplyManagement
{
    public class VerificationApprovalTemplateValue : BaseEntity
    {
        public int VerificationApprovalTemplateId { get; set; }
        public int StudyLevelFormVariableId { get; set; }
        public string Value { get; set; }
        public bool IsNa { get; set; }
        public VerificationApprovalTemplate VerificationApprovalTemplate { get; set; }
        public StudyLevelFormVariable StudyLevelFormVariable { get; set; }
        public List<VerificationApprovalTemplateValueChild> Children { get; set; }
    }
}
