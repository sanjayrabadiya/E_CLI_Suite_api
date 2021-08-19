using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;

namespace GSC.Data.Dto.SupplyManagement
{
   public class VerificationApprovalTemplateValueChildDto : BaseDto
    {
        public int VerificationApprovalTemplateValueId { get; set; }
        public int VariableValueId { get; set; }
        public string Value { get; set; }
        public VariableValue VariableValue { get; set; }
    }
}
