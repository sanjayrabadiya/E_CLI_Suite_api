
using GSC.Common.Base;


namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementKitNumberSettingsRole : BaseEntity
    {
        public int SupplyManagementKitNumberSettingsId { get; set; }
        public int RoleId { get; set; }
        
        public SupplyManagementKitNumberSettings SupplyManagementKitNumberSettings { get; set; }
        

    }
}
