using GSC.Common.Base;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Master;
using GSC.Helper;


namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementUnblindTreatment : BaseEntity
    {
        public int RandomizationId { get; set; }
        public TreatmentUnblindType TypeofUnblind { get; set; }
        public int RoleId { get; set; }
        public int? AuditReasonId { get; set; }
        public AuditReason AuditReason { get; set; }
        public string ReasonOth { get; set; }
        public Randomization Randomization { get; set; }

        public string IpAddress { get; set; }

        public string TimeZone { get; set; }

    }
}
