using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementKitDosePriority : BaseEntity
    {
        public int ProjectId { get; set; }

        public decimal Dose { get; set; }


        public DosePriority DosePriority { get; set; }

        //public Project Project { get; set; }
    }
}
