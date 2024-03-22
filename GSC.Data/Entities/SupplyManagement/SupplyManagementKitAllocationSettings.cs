using GSC.Common.Base;
using GSC.Data.Entities.Project.Design;


namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementKitAllocationSettings : BaseEntity
    {
        public int ProjectDesignVisitId { get; set; }
        public int NoOfImp { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public int Days { get; set; }
        public int PharmacyStudyProductTypeId { get; set; }

        public string IpAddress { get; set; }

        public string TimeZone { get; set; }

        public bool IsManual { get; set; }
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
       
    }
}
