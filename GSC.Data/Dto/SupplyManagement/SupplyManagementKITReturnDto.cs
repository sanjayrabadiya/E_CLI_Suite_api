using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Helper;
using System;
using System.Collections.Generic;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementKITReturnDto : BaseDto
    {
        public int SupplyManagementKITDetailId { get; set; }
        public int ReturnImp { get; set; }
        public int? AuditReasonId { get; set; }
        public AuditReason AuditReason { get; set; }
        public string ReasonOth { get; set; }
        public string Commnets { get; set; }

    }

    public class SupplyManagementKITReturnGridDto : BaseAuditDto
    {
        public string KitNo { get; set; }
        public string StudyCode { get; set; }
        public string SiteCode { get; set; }
        public string Reason { get; set; }
        public string ReasonOth { get; set; }
        public string VisitName { get; set; }
        public int? NoOfImp { get; set; }
        public int? ReturnImp { get; set; }
        public string ProductTypeName { get; set; }
        public int SupplyManagementKITDetailId { get; set; }
        public int SupplyManagementKITSeriesId { get; set; }
        public int? SupplyManagementKITReturnId { get; set; }
        public int? SupplyManagementKITReturnSeriesId { get; set; }
        public int? SupplyManagementKITReturnVerificationId { get; set; }
        public int? SupplyManagementKITReturnVerificationSeriesId { get; set; }
        public string ScreeningNo { get; set; }
        public string RandomizationNo { get; set; }
        public int? RandomizationId { get; set; }
        public int? ProjectDesignVisitId { get; set; }
        public KitStatus Status { get; set; }

        public KitStatus? PrevStatus { get; set; }

        public string ReturnReason { get; set; }

        public int? SiteId { get; set; }

        public int? ToSiteId { get; set; }

        public string ReturnBy { get; set; }

        public DateTime? ReturnDate { get; set; }

        public int? ActionBy { get; set; }

        public DateTime? ActionDate { get; set; }

        public string ActionByName { get; set; }

        public string StatusName { get; set; }

        public bool? IsUnUsed { get; set; }

        public string ReturnVerificationBy { get; set; }

        public DateTime? ReturnVerificationDate { get; set; }

        public string ReturnVerificationReason { get; set; }

        public string ReturnVerificationReasonOth { get; set; }

        public string IpAddressReturn { get; set; }

        public string TimeZoneReturn { get; set; }

        public string IpAddressVerification{ get; set; }

        public string TimeZoneVerification { get; set; }

        public bool Isdisable { get; set; }

        public string Barcode { get; set; }


    }

    public class SupplyManagementKITReturnDtofinal
    {
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public List<SupplyManagementKITReturnGridDto> list { get; set; }

        public bool? IsUnUsed { get; set; }
        public KitStatusRandomization TypeOfKitReturn { get; set; }

        public int NoOfKitReturn { get;set; }

        public int? siteId { get; set; }

        public int ProjectId { get; set; }

        public int? VisitId { get; set; }
    }
    public class SupplyManagementKITDiscardGridDto : BaseAuditDto
    {
        public string KitNo { get; set; }
        public string StudyCode { get; set; }
        public string SiteCode { get; set; }
        public string Reason { get; set; }
        public string ReasonOth { get; set; }
        public string VisitName { get; set; }
        public int? NoOfImp { get; set; }
        public int? ReturnImp { get; set; }
        public string ProductTypeName { get; set; }
        public int SupplyManagementKITDetailId { get; set; }

        public int? SupplyManagementKITDiscardId { get; set; }

        public string ScreeningNo { get; set; }
        public string RandomizationNo { get; set; }
        public int? RandomizationId { get; set; }
        public int? ProjectDesignVisitId { get; set; }
        public KitStatus Status { get; set; }

        public string ReturnReason { get; set; }

        public int? SiteId { get; set; }

        public int? SupplyManagementKITReturnId { get; set; }

        public string ReturnBy { get; set; }

        public DateTime? ReturnDate { get; set; }

        public string DiscardBy { get; set; }
        public DateTime? DiscardDate { get; set; }

        public bool? IsUnUsed { get; set; }

        public string IpAddress { get; set; }

        public string TimeZone { get; set; }

        public bool Isdisable { get; set; }

        public string Barcode { get; set; }

    }

    public class SupplyManagementKITDiscardDtofinal
    {
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public List<SupplyManagementKITDiscardGridDto> list { get; set; }
    }
    public class SupplyManagementKITReturnVerificationDto
    {
        public int SupplyManagementKITDetailId { get; set; }
        public KitStatus Status { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
    }
    public class SupplyManagementKITReturnVerificationSequenceDto
    {
        public int SupplyManagementKITSeriesId { get; set; }
        public KitStatus Status { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
    }

}
