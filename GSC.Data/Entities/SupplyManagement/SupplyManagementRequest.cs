using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.Design;
using System.ComponentModel.DataAnnotations.Schema;


namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementRequest : BaseEntity, ICommonAduit
    {

        public bool IsSiteRequest { get; set; }
        public int? FromProjectId { get; set; }
        public int? ToProjectId { get; set; }
        public int? StudyProductTypeId { get; set; }
        public int RequestQty { get; set; }
        public int? VisitId { get; set; }
        public string IpAddress { get; set; }

        public string TimeZone { get; set; }

        [ForeignKey("FromProjectId")]
        public GSC.Data.Entities.Master.Project FromProject { get; set; }

        [ForeignKey("ToProjectId")]
        public GSC.Data.Entities.Master.Project ToProject { get; set; }

        [ForeignKey("StudyProductTypeId")]
        public PharmacyStudyProductType? PharmacyStudyProductType { get; set; }

        [ForeignKey("VisitId")]
        public ProjectDesignVisit ProjectDesignVisit { get; set; }

    }   
}
