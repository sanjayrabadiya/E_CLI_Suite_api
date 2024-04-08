using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
namespace GSC.Data.Entities.CTMS
{
    public class CtmsApprovalWorkFlow : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public TriggerType TriggerType { get; set; }
        public string EmailTemplate { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        public int RoleId { get; set; }
        public Master.Project Project { get; set; }
    }
}