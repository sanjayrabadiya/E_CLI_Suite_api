using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
using GSC.Helper;
using GSC.Shared.DocumentService;
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

        public int? SupplyManagementKITReturnId { get; set; }

        public string ScreeningNo { get; set; }
        public string RandomizationNo { get; set; }
        public int? RandomizationId { get; set; }
        public int? ProjectDesignVisitId { get; set; }
        public KitStatus Status { get; set; }

        public string ReturnReason { get; set; }

        public int? SiteId { get; set; }

        public string ReturnBy { get; set; }

        public DateTime? ReturnDate { get; set; }

    }

    public class SupplyManagementKITReturnDtofinal
    {
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public List<SupplyManagementKITReturnGridDto> list { get; set; }
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


    }

    public class SupplyManagementKITDiscardDtofinal
    {
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public List<SupplyManagementKITDiscardGridDto> list { get; set; }
    }

}
