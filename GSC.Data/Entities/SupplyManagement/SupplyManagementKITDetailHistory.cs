using GSC.Common.Base;
using GSC.Data.Entities.Attendance;
using GSC.Helper;


namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementKITDetailHistory : BaseEntity
    {
        public int SupplyManagementKITDetailId { get; set; }
        public KitStatus? Status { get; set; }
        public int RoleId { get; set; }

        public int? SupplyManagementShipmentId { get; set; }

        public int? RandomizationId { get; set; }

        public SupplyManagementKITDetail SupplyManagementKITDetail { get; set; }

        public Randomization Randomization { get; set; }

    }
}
