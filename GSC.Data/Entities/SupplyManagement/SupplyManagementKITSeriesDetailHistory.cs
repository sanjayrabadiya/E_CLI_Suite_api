
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using GSC.Shared.Generic;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementKITSeriesDetailHistory : BaseEntity
    {
        public int SupplyManagementKITSeriesId { get; set; }
        public KitStatus? Status { get; set; }
        public int RoleId { get; set; }

        public int? SupplyManagementShipmentId { get; set; }

        public int? RandomizationId { get; set; }

        public Randomization Randomization { get; set; }

        public SupplyManagementKITSeries SupplyManagementKITSeries { get; set; }
        

    }
}
