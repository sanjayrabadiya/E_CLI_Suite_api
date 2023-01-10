using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System.Collections.Generic;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementKITDetailHistoryDto : BaseAuditDto
    {
        public int SupplyManagementKITDetailId { get; set; }
        public KitStatus? Status { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }

        public string StatusName { get; set; }

        public string KitNo { get; set; }

    }
   
}
