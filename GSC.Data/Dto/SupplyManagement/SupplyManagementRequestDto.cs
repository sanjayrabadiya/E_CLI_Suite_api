using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementRequestDto : BaseDto
    {
        public bool IsSiteRequest { get; set; }
        public int? FromProjectId { get; set; }
        public int? ToProjectId { get; set; }
        public int StudyProductTypeId { get; set; }
        public int RequestQty { get; set; }
    }
    public class SupplyManagementRequestGridDto : BaseAuditDto
    {
      
        public bool IsSiteRequest { get; set; }
        public int? FromProjectId { get; set; }
        public int? ToProjectId { get; set; }
        public int StudyProductTypeId { get; set; }
        public int RequestQty { get; set; }

        public int ApprovedQty { get; set; }

        public int SupplyManagementShipmentId { get; set; }

        public string Status { get; set; }

        public string StudyProductTypeName { get; set; }

        public string StudyProductTypeUnitName { get; set; }

        public string StudyProjectCode { get; set; }

        public string FromProjectCode { get; set; }

        public string ToProjectCode { get; set; }

        public DateTime? ApproveRejectDateTime { get; set; }

        public string ApproveRejectBy { get; set; }

        public string AuditReason { get; set; }

        public string ReasonOth { get; set; }

        
    }

}
