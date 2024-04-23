using GSC.Data.Entities.Common;
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
        public string Comment { get; set; }
        public string ReplyComment { get; set; }
        public bool IsReply { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}
