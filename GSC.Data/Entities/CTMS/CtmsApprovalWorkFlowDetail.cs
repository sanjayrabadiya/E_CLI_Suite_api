using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;
namespace GSC.Data.Entities.CTMS
{
    public class CtmsApprovalWorkFlowDetail : BaseEntity, ICommonAduit
    {
        public int CtmsApprovalWorkFlowId { get; set; }
        public int UserId { get; set; }
        public User Users { get; set; }
        public CtmsApprovalWorkFlow ctmsApprovalWorkFlow { get; set; }
    }
}
