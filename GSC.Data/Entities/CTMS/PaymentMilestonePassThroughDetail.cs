using GSC.Common.Base;
using GSC.Common.Common;
namespace GSC.Data.Entities.CTMS
{
    public class PaymentMilestonePassThroughDetail : BaseEntity, ICommonAduit
    {
        public int PaymentMilestoneId { get; set; }
        public int PassThroughCostId { get; set; }
        public PaymentMilestone PaymentMilestone { get; set; }
        public PassThroughCost PassThroughCost { get; set; }
    }
}
