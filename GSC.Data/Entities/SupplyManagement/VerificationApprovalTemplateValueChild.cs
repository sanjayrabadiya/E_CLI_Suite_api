using GSC.Common.Base;
using GSC.Data.Entities.Project.StudyLevelFormSetup;

namespace GSC.Data.Entities.SupplyManagement
{
    public class VerificationApprovalTemplateValueChild : BaseEntity
    {
        public int VerificationApprovalTemplateValueId { get; set; }
        public int StudyLevelFormVariableValueId { get; set; }
        public string Value { get; set; }
        public StudyLevelFormVariableValue StudyLevelFormVariableValue { get; set; }
    }
}
