
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
    public class SupplyManagementEmailConfigurationDetailHistory : BaseEntity
    {
        public int SupplyManagementEmailConfigurationDetailId { get; set; }
        public SupplyManagementEmailConfigurationDetail SupplyManagementEmailConfigurationDetail { get; set; }
       


    }
}
