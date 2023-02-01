
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
    public class SupplyManagementEmailConfiguration : BaseEntity
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }

        public string EmailBody { get; set; }
        public SupplyManagementEmailTriggers Triggers { get; set; }
        public int Days { get; set; }
        public bool IsActive { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public Entities.Master.Project Project { get; set; }
        


    }
}
