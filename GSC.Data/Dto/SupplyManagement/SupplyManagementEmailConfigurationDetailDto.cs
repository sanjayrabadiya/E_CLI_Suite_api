using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.SupplyManagement;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System.Collections.Generic;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementEmailConfigurationDetailDto : BaseDto
    {
        public int SupplyManagementEmailConfigurationId { get; set; }
        public int RoleId { get; set; }
        public int UserId { get; set; }

    }
}
