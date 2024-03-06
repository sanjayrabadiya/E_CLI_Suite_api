using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementKIT : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public int? ToSiteId { get; set; }
        public int? ProjectDesignVisitId { get; set; }
        public int NoOfImp { get; set; }
        public int NoofPatient { get; set; }
        public int? NoOfKits { get; set; }
        public int? TotalUnits { get; set; }
        public int PharmacyStudyProductTypeId { get; set; }
        public Entities.Master.Project Project { get; set; }

        public int? Days { get; set; }
        public int? ProductReceiptId { get; set; }

        public decimal? Dose { get; set; }

        [ForeignKey("SiteId")]
        public Entities.Master.Project Site { get; set; }
        public int? AuditReasonId { get; set; }
        public AuditReason AuditReason { get; set; }
        public string ReasonOth { get; set; }
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
        public PharmacyStudyProductType PharmacyStudyProductType { get; set; }

        public string TimeZone { get; set; }
        public string IpAddress { get; set; }

    }
}
