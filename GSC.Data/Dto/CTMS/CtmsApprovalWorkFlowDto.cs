using GSC.Data.Entities.Common;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace GSC.Data.Dto.CTMS
{
    public class CtmsApprovalWorkFlowDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int RoleId { get; set; }
        public string EmailTemplate { get; set; }
        public TriggerType TriggerType { get; set; }

        [NotMapped]
        public IList<CtmsApprovalWorkFlowDetail> CtmsApprovalWorkFlowDetails { get; set; } = null;
        public IList<int> UserIds { get; set; }
    }

    public class CtmsApprovalWorkFlowGridDto : BaseAuditDto
    {
        public string ProjectCode { get; set; }
        public int ProjectId { get; set; }
        public string RoleName { get; set; }
        public int RoleId { get; set; }
        public string Users { get; set; }
        public string EmailTemplate { get; set; }
        public string TriggerTypeName { get; set; }
        public TriggerType TriggerType { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}
