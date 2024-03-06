
using GSC.Common.Base;

using GSC.Data.Entities.Master;


namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementKITReturn : BaseEntity
    {
        public int SupplyManagementKITDetailId { get; set; }
        public int? ReturnImp { get; set; }
        public int? AuditReasonId { get; set; }
        public AuditReason AuditReason { get; set; }
        public string ReasonOth { get; set; }
        public string Commnets { get; set; }

        public string IpAddress { get; set; }

        public string TimeZone { get; set; }
        public SupplyManagementKITDetail SupplyManagementKITDetail { get; set; }
       
    }
}
