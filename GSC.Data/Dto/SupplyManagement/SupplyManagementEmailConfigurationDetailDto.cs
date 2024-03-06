using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementEmailConfigurationDetailDto : BaseDto
    {
        public int SupplyManagementEmailConfigurationId { get; set; }
        public int RoleId { get; set; }
        public int UserId { get; set; }

    }
}
