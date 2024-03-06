using GSC.Helper;


namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementShipmentApprovalDto
    {
        public int SupplyManagementRequestId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string Comments { get; set; }
        public SupplyManagementApprovalStatus Status { get; set; }
       
        
    }
}
