
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementVisitKITDetail : BaseEntity
    {
        public string KitNo { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public int RandomizationId { get; set; }

        public int? SupplyManagementShipmentId { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string ProductCode { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        public AuditReason AuditReason { get; set; }
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
        public Randomization Randomization { get; set; }
        public int? SupplyManagementKITDetailId { get; set; }
        public SupplyManagementKITDetail SupplyManagementKITDetail { get; set; }

    }
}
