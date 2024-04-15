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
    public class CtmsWorkflowApprovalDto : BaseDto
    {
        public int StudyPlanId { get; set; }
        public int ProjectId { get; set; }
        public int SenderId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool? IsApprove { get; set; }
        public int? CtmsWorkflowApprovalId { get; set; }
        public DateTime? SendDate { get; set; }
        public DateTime? ActionDate { get; set; }
        public TriggerType TriggerType { get; set; }
        public string SenderComment { get; set; }
        public string ApproverComment { get; set; }
    }
}
