using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementReceiptDto : BaseDto
    {
        public int SupplyManagementShipmentId { get; set; }
        public bool WithIssue { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string Description { get; set; }

        public List<KitListApproved> Kits { get; set; }
    }
    public class SupplyManagementReceiptGridDto : BaseAuditDto
    {
        public int? FromProjectId { get; set; }
        public int? ToProjectId { get; set; }

        public int SupplyManagementShipmentId { get; set; }
        public bool? WithIssue { get; set; }
        public string ReasonOth { get; set; }
        public int? ApprovedQty { get; set; }
        public string StatusName { get; set; }
        public SupplyMangementShipmentStatus Status { get; set; }
        public string StudyProjectCode { get; set; }
        public string FromProjectCode { get; set; }
        public string ToProjectCode { get; set; }
        public DateTime? ApproveRejectDateTime { get; set; }
        public string AuditReason { get; set; }
        public string ShipmentNo { get; set; }
        public string CourierName { get; set; }
        public DateTime? CourierDate { get; set; }
        public string CourierTrackingNo { get; set; }
        public string Description { get; set; }
        public string ApproveRejectBy { get; set; }

        public string StudyProductTypeName { get; set; }

        public string StudyProductTypeUnitName { get; set; }

        public string ShipmentReason { get; set; }

        public string ShipmentReasonOth { get; set; }

        public ProductUnitType? ProductUnitType { get; set; }

        public string WithIssueName { get; set; }
    }

    public class SupplyManagementReceiptHistoryGridDto : BaseAuditDto
    {
        public int RequestQty { get; set; }
        public string Status { get; set; }

        public string RequestType { get; set; }

        public string ProductTypeName { get; set; }

        public string  StudyProductTypeUnitName { get; set; }

        public string StudyProjectCode { get; set; }

        public string FromProjectCode { get; set; }

        public string ToProjectCode { get; set; }

        public string VisitName { get; set; }

        public string ActivityBy { get; set; }

        public DateTime? ActivityDate { get; set; }
    }
    public class KitAllocatedList
    {
        public int Id { get; set; }
        public string KitNo { get; set; }
        public string VisitName { get; set; }
        public string SiteCode { get; set; }

        public string Comments { get; set; }

        public string Status { get; set; }

       
    }
}
