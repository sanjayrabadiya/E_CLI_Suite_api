
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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
