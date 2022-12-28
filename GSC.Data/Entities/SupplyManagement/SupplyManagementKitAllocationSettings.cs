
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
    public class SupplyManagementKitAllocationSettings : BaseEntity
    {
        public int VisitId { get; set; }
        public int NoOfImp { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
       
    }
}
