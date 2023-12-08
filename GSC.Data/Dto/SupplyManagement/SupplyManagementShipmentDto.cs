using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementShipmentDto : BaseDto
    {
        public int SupplyManagementRequestId { get; set; }
        public SupplyMangementShipmentStatus Status { get; set; }
        public int ApprovedQty { get; set; }
        public string ShipmentNo { get; set; }
        public string CourierName { get; set; }
        public DateTime? CourierDate { get; set; }
        public string CourierTrackingNo { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }

        public DateTime? EstimatedCourierDate { get; set; }
        public List<KitListApprove> Kits { get; set; }
    }
    public class SupplyManagementShipmentGridDto : BaseAuditDto
    {

        public bool IsSiteRequest { get; set; }
        public int? FromProjectId { get; set; }
        public int? ToProjectId { get; set; }
        public int? RequestQty { get; set; }

        public int? ApprovedQty { get; set; }

        public int SupplyManagementRequestId { get; set; }

        public string StatusName { get; set; }


        public string StudyProjectCode { get; set; }

        public int ProjectId { get; set; }

        public string FromProjectCode { get; set; }

        public string ToProjectCode { get; set; }


        public string AuditReason { get; set; }
        public string ReasonOth { get; set; }
        public string ShipmentNo { get; set; }
        public string CourierName { get; set; }
        public DateTime? CourierDate { get; set; }
        public string CourierTrackingNo { get; set; }
        public SupplyMangementShipmentStatus Status { get; set; }

        public int? AuditReasonId { get; set; }

        public DateTime? RequestDate { get; set; }

        public string RequestBy { get; set; }

        public string StudyProductTypeName { get; set; }

        public string StudyProductTypeUnitName { get; set; }

        public ProductUnitType ProductUnitType { get; set; }

        public string SiteRequest { get; set; }
        public int? StudyProductTypeId { get; set; }

        public bool IsWorkflowApproval { get; set; }

        public int WorkflowId { get; set; }
        public string WorkflowApprovalName { get; set; }

        public DateTime? WorkflowDate { get; set; }

        public string WorkflowApprovalRoleName { get; set; }

        public string WorkflowComments { get; set; }

        public SupplyManagementApprovalStatus WorkflowStatus { get; set; }

        public string IpAddress { get; set; }

        public string TimeZone { get; set; }

    }

}
