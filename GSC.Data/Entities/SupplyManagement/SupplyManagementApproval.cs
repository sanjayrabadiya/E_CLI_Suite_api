using GSC.Common.Base;
using GSC.Data.Entities.Master;
using GSC.Helper;


namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementApproval : BaseEntity
    {
        public int ProjectId { get; set; }
        public string EmailTemplate { get; set; }
        public SupplyManagementApprovalType ApprovalType { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public Entities.Master.Project Project { get; set; }
        public AuditReason AuditReason { get; set; }

        public string IpAddress { get; set; }

        public string TimeZone { get; set; }
        public int RoleId { get; set; }
    }
}
