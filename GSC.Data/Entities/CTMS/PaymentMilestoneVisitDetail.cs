using GSC.Common.Base;
using GSC.Common.Common;
namespace GSC.Data.Entities.CTMS
{
    public class PaymentMilestoneVisitDetail : BaseEntity, ICommonAduit
    {
        public int PatientMilestoneId { get; set; }
        public int PatientCostId { get; set; }
        public PatientMilestone PatientMilestone { get; set; }
        public PatientCost PatientCost { get; set; }
    }
}
