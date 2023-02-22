
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
    public class SupplyManagementKITReturnVerificationSeries : BaseEntity
    {
        public int SupplyManagementKITSeriesId { get; set; }
        public int? AuditReasonId { get; set; }
        public AuditReason AuditReason { get; set; }
        public string ReasonOth { get; set; }

        public KitStatus Status { get; set; }
        public SupplyManagementKITSeries SupplyManagementKITSeries { get; set; }
       
    }
}
