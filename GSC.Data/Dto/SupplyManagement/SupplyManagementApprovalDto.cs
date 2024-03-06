using GSC.Data.Entities.Common;
using GSC.Data.Entities.SupplyManagement;
using GSC.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementApprovalDto : BaseDto
    {
        public int RoleId { get; set; }
        public int ProjectId { get; set; }
        public string EmailTemplate { get; set; }
        public SupplyManagementApprovalType ApprovalType { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }

        [NotMapped]
        public IList<SupplyManagementApprovalDetails> SupplyManagementApprovalDetails { get; set; } = null;

        public IList<int> UserIds { get; set; }

    }

    public class SupplyManagementApprovalGridDto : BaseAuditDto
    {
        public string ProjectCode { get; set; }
        public string RoleName { get; set; }
        public string Users { get; set; }
        public int ProjectId { get; set; }
        public int RoleId { get; set; }
        public string EmailTemplate { get; set; }

        public string ApprovalTypeName { get; set; }
        public SupplyManagementApprovalType ApprovalType { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }

        public string AuditReasonName { get; set; }

        public string IpAddress { get; set; }
        public string TimeZone { get; set; }


    }
   
}
