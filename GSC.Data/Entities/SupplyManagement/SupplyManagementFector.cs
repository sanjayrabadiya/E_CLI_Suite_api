using GSC.Common.Base;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Helper;
using System.Collections.Generic;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementFector : BaseEntity
    {
        public int ProjectId { get; set; }
        public string Formula { get; set; }
        public GSC.Data.Entities.Master.Project Project { get; set; }
        public List<SupplyManagementFectorDetail> FectorDetailList { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
    }
}
