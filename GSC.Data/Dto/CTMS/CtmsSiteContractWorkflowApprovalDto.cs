using GSC.Data.Entities.Common;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Data.Dto.CTMS
{
    public class CtmsSiteContractWorkflowApprovalDto : BaseDto
    {
        public int SiteContractId { get; set; }
        public int ProjectId { get; set; }
        public int SiteId { get; set; }
        public int SenderId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool? IsApprove { get; set; }
        public int? CtmsSiteContractWorkflowApprovalId { get; set; }
        public DateTime? SendDate { get; set; }
        public DateTime? ActionDate { get; set; }
        public TriggerType TriggerType { get; set; }
        public string SenderComment { get; set; }
        public string ApproverComment { get; set; }
    }

    public class CtmsSiteContractWorkflowApprovalGridDto : BaseAuditDto
    {
        public int SiteContractId { get; set; }
        public int ProjectId { get; set; }
        public int SiteId { get; set; }
        public int SenderId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool? IsApprove { get; set; }
        public int? CtmsSiteContractWorkflowApprovalId { get; set; }
        public DateTime? SendDate { get; set; }
        public DateTime? ActionDate { get; set; }
        public TriggerType TriggerType { get; set; }
        public string SenderComment { get; set; }
        public string ApproverComment { get; set; }
        public string SenderName { get; set; }
        public string ApproverName { get; set; }
        public string ApproverRole { get; set; }
        public string ProjectName { get; set; }
        public bool HasChild { get; set; }
    }
}
