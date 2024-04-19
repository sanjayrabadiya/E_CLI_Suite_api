using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
namespace GSC.Data.Entities.CTMS
{
    public class PatientCost : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int? ProcedureId { get; set; }
        public int? ProjectDesignVisitId { get; set; }
        public string VisitName { get; set; }
        public string VisitDescription { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Cost { get; set; }
        public int? CurrencyRateId { get; set; }
        public decimal? FinalCost { get; set; }
        public int? CurrencyId { get; set; }
        public bool IfPull { get; set; }
        public int PatientCount { get; set; }
        public Master.Project Project { get; set; }
        public Procedure Procedure { get; set; }
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
        public CurrencyRate CurrencyRate { get; set; }
        public Currency Currency { get; set; }
    }
}
