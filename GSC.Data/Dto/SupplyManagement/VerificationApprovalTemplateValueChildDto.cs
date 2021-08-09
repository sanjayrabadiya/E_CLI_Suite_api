using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.SupplyManagement
{
   public class VerificationApprovalTemplateValueChildDto : BaseDto
    {
        public int ScreeningTemplateValueId { get; set; }
        public int ProjectDesignVariableValueId { get; set; }
        public string Value { get; set; }
        public ProjectDesignVariableValue ProjectDesignVariableValue { get; set; }
    }
}
