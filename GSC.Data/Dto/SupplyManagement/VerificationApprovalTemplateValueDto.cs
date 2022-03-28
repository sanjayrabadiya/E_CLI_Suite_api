using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Data.Entities.SupplyManagement;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.SupplyManagement
{
    public class VerificationApprovalTemplateValueDto : BaseDto
    {
        public int VerificationApprovalTemplateId { get; set; }
        public int StudyLevelFormVariableId { get; set; }
        public string Value { get; set; }
        public string ValueName { get; set; }
        public string OldValue { get; set; }
        public bool IsNa { get; set; }
        public ICollection<VerificationApprovalTemplateValueChildDto> Children { get; set; }
        public VerificationApprovalTemplate VerificationApprovalTemplate { get; set; }
        public StudyLevelFormVariable StudyLevelFormVariable { get; set; }
    }

    public class VerificationApprovalTemplateValueBasic
    {
        public int VerificationApprovalTemplateId { get; set; }
        public int StudyLevelFormVariableId { get; set; }
        public int Id { get; set; }
        public string Value { get; set; }
        public bool IsNa { get; set; }
        public int? UserRoleId { get; set; }
        public ICollection<VerificationApprovalTemplateValueChild> Children { get; set; }

    }
}
