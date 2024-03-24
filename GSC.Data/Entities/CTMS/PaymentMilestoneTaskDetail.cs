using GSC.Common.Base;
using GSC.Common.Common;
namespace GSC.Data.Entities.CTMS
{
    public class PaymentMilestoneTaskDetail : BaseEntity, ICommonAduit
    {
        public int PaymentMilestoneId { get; set; }
        public int StudyPlanTaskId { get; set; }
        public PaymentMilestone PaymentMilestone { get; set; }
        public StudyPlanTask StudyPlanTask { get; set; }

    }
}
