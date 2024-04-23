using GSC.Common.Base;
using GSC.Common.Common;
namespace GSC.Data.Entities.CTMS
{
    public class PaymentMilestonePassThroughDetail : BaseEntity, ICommonAduit
    {
        public int PassthroughMilestoneId { get; set; }
        public int PassThroughCostId { get; set; }
        public PassthroughMilestone PassthroughMilestone { get; set; }
        public PassThroughCost PassThroughCost { get; set; }
    }
}
