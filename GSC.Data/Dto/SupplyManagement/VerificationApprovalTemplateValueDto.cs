using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.SupplyManagement;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.SupplyManagement
{
    public class VerificationApprovalTemplateValueDto : BaseDto
    {
        public int VerificationApprovalTemplateId { get; set; }
        public int VariableId { get; set; }
        public string Value { get; set; }
        public bool IsNa { get; set; }
        public VerificationApprovalTemplate VerificationApprovalTemplate { get; set; }
        public Variable Variable { get; set; }
    }
}
