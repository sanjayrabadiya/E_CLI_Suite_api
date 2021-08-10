using GSC.Common.Base;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.SupplyManagement
{
    public class VerificationApprovalTemplateValueChild : BaseEntity
    {
        public int VerificationApprovalTemplateValueId { get; set; }
        public int VariableValueId { get; set; }
        public string Value { get; set; }
        public VariableValue VariableValue { get; set; }
    }
}
