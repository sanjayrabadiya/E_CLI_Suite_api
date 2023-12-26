using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.SupplyManagement;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementKITSeriesDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public int NoofPatient { get; set; }
        public string TreatmentType { get; set; }
        public string KitNo { get; set; }
        public int? SupplyManagementShipmentId { get; set; }
        public int? RandomizationId { get; set; }
        public KitStatus Status { get; set; }
        public KitStatus? PrevStatus { get; set; }

        public DateTime? KitExpiryDate { get; set; }

        [NotMapped]
        public IList<SupplyManagementKITSeriesDetailDto> SupplyManagementKITSeriesDetail { get; set; }


    }

    public class SupplyManagementKITSeriesDetailDto : BaseDto
    {
        public int SupplyManagementKITSeriesId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public int PharmacyStudyProductTypeId { get; set; }
        public int? RandomizationId { get; set; }
        public int NoOfImp { get; set; }
        public int NoofPatient { get; set; }
        public int TotalUnits { get; set; }
        public bool? IsUnUsed { get; set; }
        public int Days { get; set; }
        public int ProductReceiptId { get; set; }

        public string Barcode { get; set; }

    }

    public class SupplyManagementKITSeriesGridDto : BaseAuditDto
    {
        public string KitNo { get; set; }
        public string StudyCode { get; set; }
        public string SiteCode { get; set; }
        public int NoofPatient { get; set; }
        public string TreatmentType { get; set; }
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public int? ToSiteId { get; set; }
        public int? SupplyManagementShipmentId { get; set; }
        public string RandomizationNo { get; set; }
        public int? RandomizationId { get; set; }
        public string RequestFromSite { get; set; }
        public string RequestToSiteOrStudy { get; set; }

        public string statusName { get; set; }

        public KitStatus Status { get; set; }

        //public string Reason { get; set; }

        //public string ReasonOth { get; set; }

        public DateTime? KitExpiryDate { get; set; }

        public SupplyManagementShipment SupplyManagementShipment { get; set; }

        public string TimeZone { get; set; }
        public string IpAddress { get; set; }

        public bool IsRetension { get; set; }

        public string Barcode { get; set; }

    }
    public class SupplyManagementKITSeriesDetailGridDto : BaseAuditDto
    {
        public int SupplyManagementKITSeriesId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public int PharmacyStudyProductTypeId { get; set; }
        public int? RandomizationId { get; set; }
        public int NoOfImp { get; set; }
        public int NoofPatient { get; set; }
        public int TotalUnits { get; set; }
        public bool? IsUnUsed { get; set; }
        public string ProductType { get; set; }
        public string RandomizationNo { get; set; }
        public string VisitName { get; set; }
        public string KitNo { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int? ProductReceiptId { get; set; }
        public string LotBatchNo { get; set; }

    }

    public class SupplyManagementKITSeriesDetailHistoryGridDto : BaseAuditDto
    {
        public int SupplyManagementKITSeriesId { get; set; }
        public KitStatus? Status { get; set; }
        public int RoleId { get; set; }
        public string KitNo { get; set; }
        public string StatusName { get; set; }
        public string RoleName { get; set; }

        public string FromProjectCode { get; set; }

        public string ToProjectCode { get; set; }
        public int? SupplyManagementShipmentId { get; set; }
    }

}
