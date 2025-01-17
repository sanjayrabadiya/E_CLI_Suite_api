﻿using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.SupplyManagement;
using GSC.Helper;
using System;
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
     
        public int PharmacyStudyProductTypeId { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }

        public int ProductReceiptId { get; set; }

        public int Days { get; set; }

        public decimal? Dose { get; set; }
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
       
        public string ProductTypeName { get; set; }
        public int ProjectId { get; set; }
        public int? CountryId { get; set; }
        public int? SiteId { get; set; }

        public int? ToSiteId { get; set; }

        public int SupplyManagementKITId { get; set; }

        public int? SupplyManagementShipmentId { get; set; }
        public int? PharmacyStudyProductTypeId { get; set; }
        public int? ProjectDesignVisitId { get; set; }

        public string KitStatus { get; set; }

        public KitStatus Status { get; set; }

        public string RandomizationNo { get; set; }

        public int? RandomizationId { get; set; }

        public string RequestFromSite { get; set; }

        public string RequestToSiteOrStudy { get; set; }

        public DateTime? ExpiryDate { get; set; }
        public string LotBatchNo { get; set; }

        public decimal? Dose { get; set; }
        public SupplyManagementShipment SupplyManagementShipment { get; set; }

        public string TimeZone { get; set; }
        public string IpAddress { get; set; }

        public bool IsRetension { get; set; }

        public string Barcode { get; set; }


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

        public string ProjectCode { get; set; }

        public string TreatmentType { get; set; }

        public string LotBatchNo { get; set; }

        public DateTime? RetestExpiry { get; set; }

        public string RetestExpirystr { get; set; }

        public DateTime? KitValidity { get; set; }

        public decimal? Dose { get; set; }

        public string Barcode { get; set; }
        public bool Isdisable { get; set; }
    }

    public class KitListApproved
    {
        public int Id { get; set; }
        public string KitNo { get; set; }
        public string VisitName { get; set; }
        public string SiteCode { get; set; }

        public string Comments { get; set; }

        public KitStatus Status { get; set; }

        public string LotBatchNo { get; set; }

        public DateTime? RetestExpiry { get; set; }

        public DateTime? KitValidity { get; set; }

        public List<DropDownStudyDto> StatusList { get; set; }

        public decimal? Dose { get; set; }

        public bool IsRetension { get; set; }

        public bool IsDisable { get; set; }
    }
}
