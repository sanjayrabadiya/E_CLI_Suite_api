using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System.Collections.Generic;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementKITDto : BaseDto
    {
        public int KitNo { get; set; }
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public int? ProjectDesignVisitId { get; set; }
        public int NoOfImp { get; set; }
        public int NoofPatient { get; set; }
        //public int NoOfKits { get; set; }
        //public int TotalUnits { get; set; }
        public int PharmacyStudyProductTypeId { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }


    }

    public class SupplyManagementKITGridDto : BaseAuditDto
    {
        public string KitNo { get; set; }
        public string StudyCode { get; set; }
        public string SiteCode { get; set; }
        public string Reason { get; set; }
        public string ReasonOth { get; set; }
        public string VisitName { get; set; }
        public int NoOfImp { get; set; }
        public int NoofPatient { get; set; }
        //public int NoOfKits { get; set; }
        //public int TotalUnits { get; set; }
        public string ProductTypeName { get; set; }
        public int ProjectId { get; set; }
        public int? CountryId { get; set; }
        public int? SiteId { get; set; }

        public int SupplyManagementKITId { get; set; }

        public int? SupplyManagementShipmentId { get; set; }
        public int? PharmacyStudyProductTypeId { get; set; }
        public int? ProjectDesignVisitId { get; set; }

        public string Status { get; set; }

        public string RandomizationNo { get; set; }

        public int? RandomizationId { get; set; }


    }

    public class DeleteKitDto
    {
        public List<int> list { get; set; }
    }
    public class KitListApprove
    {
        public int Id { get; set; }
        public string KitNo { get; set; }
        public string VisitName { get; set; }
        public string SiteCode { get; set; }

        public string ProductCode { get; set; }
    }

    public class KitListApproved
    {
        public int Id { get; set; }
        public string KitNo { get; set; }
        public string VisitName { get; set; }
        public string SiteCode { get; set; }

        public string Comments { get; set; }

        public KitStatus Status { get; set; }

        public List<DropDownEnum> StatusList { get; set; }
    }
}
