using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.SupplyManagement
{
    public class VerificationApprovalTemplateDto : BaseDto
    {
        public int? VariableTemplateId { get; set; }
        public int SecurityRoleId { get; set; }
        public int ProductVerificationDetailId { get; set; }
        public DateTime? ApproveOn { get; set; }
        public bool IsApprove { get; set; }
        public VariableTemplate VariableTemplate { get; set; }
        public SecurityRole SecurityRole { get; set; }
        public VerificationApprovalTemplateHistoryDto VerificationApprovalTemplateHistory { get; set; } = null;
        public IList<VerificationApprovalTemplateValueDto> VerificationApprovalTemplateValueList { get; set; }
    }

    public class VerificationApprovalTemplateBasic
    {
        public int Id { get; set; }
        public int ProductVerificationDetailId { get; set; }
        public int VariableTemplateId { get; set; }
    }
}
