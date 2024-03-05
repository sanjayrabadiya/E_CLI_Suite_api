using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
using GSC.Helper;
namespace GSC.Data.Entities.CTMS
{
    public class PassThroughCost : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int PassThroughCostActivityId { get; set; }
        public int CountryId { get; set; }
        public int UnitId { get; set; }
        public int NoOfUnit { get; set; }
        public decimal Rate { get; set; }
        public int Frequency { get; set; }
        public decimal? Total { get; set; }
        public int? CurrencyRateId { get; set; }
        public Master.Project Project { get; set; }
        public PassThroughCostActivity PassThroughCostActivity { get; set; }
        public Country Country { get; set; }
        public BudgetFlgType BudgetFlgType { get; set; }
        public Unit Unit { get; set; }
        public CurrencyRate CurrencyRate { get; set; }
    }
}
