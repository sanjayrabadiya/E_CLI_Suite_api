using GSC.Common.Base;
using GSC.Data.Entities.UserMgt;


namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementApprovalDetails : BaseEntity
    {
        public int SupplyManagementApprovalId { get; set; }
        public int UserId { get; set; }
        public User Users { get; set; }
        public SupplyManagementApproval SupplyManagementApproval { get; set; }
    }
}
