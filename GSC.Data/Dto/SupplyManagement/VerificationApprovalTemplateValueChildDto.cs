using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.StudyLevelFormSetup;

namespace GSC.Data.Dto.SupplyManagement
{
   public class VerificationApprovalTemplateValueChildDto : BaseDto
    {
        public int VerificationApprovalTemplateValueId { get; set; }
        public int StudyLevelFormVariableValueId { get; set; }
        public string Value { get; set; }
        public StudyLevelFormVariableValue StudyLevelFormVariableValue { get; set; }
    }
}
