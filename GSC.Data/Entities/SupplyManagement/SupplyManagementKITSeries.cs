
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
    public class SupplyManagementKITSeries : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public int? ToSiteId { get; set; }
        public int NoofPatient { get; set; }
        public string TreatmentType { get; set; }
        public string KitNo { get; set; }
        public int? SupplyManagementShipmentId { get; set; }
        public int? RandomizationId { get; set; }
        public int? AuditReasonId { get; set; }        
        public string ReasonOth { get; set; }
        public AuditReason AuditReason { get; set; }
        public KitStatus Status { get; set; }
        public KitStatus? PrevStatus { get; set; }

        public string Comments { get; set; }
        public Entities.Master.Project Project { get; set; }
        public SupplyManagementShipment SupplyManagementShipment { get; set; }
        public Randomization Randomization { get; set; }

        public bool? IsUnUsed { get; set; }

    }
}
