using GSC.Common.Base;
using GSC.Data.Entities.Master;

namespace GSC.Data.Entities.SupplyManagement
{
    public class VerificationApprovalTemplate:BaseEntity
    {
        public int VariableTemplateId { get; set; }
        public bool Status{ get; set; }
        public VariableTemplate VariableTemplate { get; set; }
    }
}
