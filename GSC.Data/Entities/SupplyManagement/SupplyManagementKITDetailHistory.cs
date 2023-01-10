﻿
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using GSC.Shared.Generic;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementKITDetailHistory : BaseEntity
    {
        public int SupplyManagementKITDetailId { get; set; }
        public KitStatus? Status { get; set; }
        public int RoleId { get; set; }

    }
}