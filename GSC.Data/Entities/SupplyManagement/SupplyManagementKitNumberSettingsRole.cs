
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementKitNumberSettingsRole : BaseEntity
    {
        public int SupplyManagementKitNumberSettingsId { get; set; }
        public int RoleId { get; set; }
        
        public SupplyManagementKitNumberSettings SupplyManagementKitNumberSettings { get; set; }
        

    }
}
