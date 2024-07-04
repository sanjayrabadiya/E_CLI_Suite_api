using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.Design;
namespace GSC.Data.Entities.CTMS
{
    public class PatientSiteContract : BaseEntity, ICommonAduit
    {
        public int SiteContractId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public decimal VisitTotal { get; set; }
        public decimal PayableTotal { get; set; }
       
        public SiteContract SiteContract { get; set; }
        public ProjectDesignVisit ProjectDesignVisit { get; set; } 

    }
}
