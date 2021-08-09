using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.SupplyManagement
{
    public class VerificationApprovalTemplateDto : BaseDto
    {
        public int VariableTemplateId { get; set; }
        public bool Status { get; set; }
        public VariableTemplate VariableTemplate { get; set; }
    }
}
