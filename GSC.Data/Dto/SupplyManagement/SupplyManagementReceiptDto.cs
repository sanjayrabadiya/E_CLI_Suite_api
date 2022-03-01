using GSC.Data.Entities.Common;
using GSC.Helper;
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
    }
    public class SupplyManagementReceiptGridDto : BaseAuditDto
    {
        public int? FromProjectId { get; set; }
        public int? ToProjectId { get; set; }

        public int SupplyManagementShipmentId { get; set; }
        public bool WithIssue { get; set; }        
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
    }

}
