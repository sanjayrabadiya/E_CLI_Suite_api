
using GSC.Common.Base;
using GSC.Data.Entities.Master;
using GSC.Helper;


namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementKITDiscard : BaseEntity
    {
        public int SupplyManagementKITDetailId { get; set; }
        public int? AuditReasonId { get; set; }
        public KitStatus Status { get; set; }
        public AuditReason AuditReason { get; set; }
        public string ReasonOth { get; set; }
        public SupplyManagementKITDetail SupplyManagementKITDetail { get; set; }
        public int? RoleId { get; set; }
        public string IpAddress { get; set; }

        public string TimeZone { get; set; }

    }
}
