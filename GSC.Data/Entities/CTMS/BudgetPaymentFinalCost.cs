using GSC.Common.Base;

namespace GSC.Data.Entities.CTMS
{
    public class BudgetPaymentFinalCost : BaseEntity
    {
        public int ProjectId { get; set; }
        
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
        public decimal? PercentageValue { get; set; }
        public decimal FinalTotalAmount { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        public Master.Project Project { get; set; }
    }
}
