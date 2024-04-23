using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Helper;

namespace GSC.Data.Entities.CTMS
{
    public class CtmsStudyPlanTaskComment : BaseEntity, ICommonAduit
    {
        public int CtmsWorkflowApprovalId { get; set; }
        public string Comment { get; set; }
        public string ReplyComment { get; set; }
        public bool IsReply { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}
