using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;

namespace GSC.Data.Entities.CTMS
{
    public class CtmsSiteContractWorkflowApproval : BaseEntity, ICommonAduit
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
        public SiteContract SiteContract { get; set; }
        public Data.Entities.Master.Project Project { get; set; }
        public User Sender { get; set; }
        public User User { get; set; }
        public SecurityRole Role { get; set; }
    }
}
