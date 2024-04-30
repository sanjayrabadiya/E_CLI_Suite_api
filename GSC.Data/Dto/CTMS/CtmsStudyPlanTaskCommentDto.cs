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
    public class CtmsStudyPlanTaskCommentDto : BaseDto
    {
        public int CtmsWorkflowApprovalId { get; set; }
        public int StudyPlanTaskId { get; set; }
        public int StudyPlanId { get; set; }
        public int ProjectId { get; set; }
        public string Comment { get; set; }
        public string ReplyComment { get; set; }
        public bool IsReply { get; set; }
        public bool FinalReply { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        public bool HasChild { get; set; }
    }

    public class CtmsStudyPlanTaskCommentGridDto : BaseAuditDto
    {
        public int CtmsWorkflowApprovalId { get; set; }
        public int StudyPlanTaskId { get; set; }
        public string Comment { get; set; }
        public string ReplyComment { get; set; }
        public bool IsReply { get; set; }
        public bool FinalReply { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }

        public int SenderId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool? IsApprove { get; set; }
        public int? ChildId { get; set; }
        public DateTime? SendDate { get; set; }
        public DateTime? ActionDate { get; set; }
        public TriggerType TriggerType { get; set; }
        public string SenderName { get; set; }
        public string ApproverName { get; set; }
        public string ApproverRole { get; set; }
        public string ProjectName { get; set; }
        public bool HasChild { get; set; }
    }
}
