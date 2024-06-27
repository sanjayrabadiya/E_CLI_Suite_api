using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
namespace GSC.Data.Entities.CTMS
{
    public class SitePayment : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int SiteId { get; set; }
        public int? CountryId { get; set; }
        public BudgetPaymentType BudgetPaymentType { get; set; }
        public int? ProjectDesignVisitId { get; set; }
        public decimal? VisitTotal { get; set; }
        public int? NoOfPatient { get; set; }    
        public int? PassThroughCostActivityId { get; set; }
        public decimal? PassThroughTotalRate { get; set; }
        public int? UnitId { get; set; }
        public int? NoOfUnitPatient { get; set; }
        public int? Frequency { get; set; }
        public decimal? PayableAmount { get; set;}
        public string Remark { get; set; }

        public Master.Project Project { get; set; }
        public Country Country { get; set; }
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
        public PassThroughCostActivity PassThroughCostActivity { get; set; }
        public Unit Unit { get; set; }

    }
}
