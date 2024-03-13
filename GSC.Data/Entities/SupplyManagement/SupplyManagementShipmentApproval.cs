using GSC.Common.Base;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;


namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementShipmentApproval : BaseEntity
    {
        public int SupplyManagementRequestId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }

        public string Comments { get; set; }
        public SupplyManagementApprovalStatus Status { get; set; }
        public User Users { get; set; }
        
    }
}
