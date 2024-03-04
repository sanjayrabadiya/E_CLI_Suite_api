using GSC.Common.Base;
using GSC.Common.Common;
namespace GSC.Data.Entities.CTMS
{
    public class PaymentMilestoneVisitDetail : BaseEntity, ICommonAduit
    {
        public int PaymentMilestoneId { get; set; }
        public int PatientCostId { get; set; }
        public PaymentMilestone PaymentMilestone { get; set; }
        public PatientCost PatientCost { get; set; }
    }
}
