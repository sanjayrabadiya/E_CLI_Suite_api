using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System;

namespace GSC.Data.Entities.Master
{
    public class ProjectStatus : BaseEntity
    {
        public int ProjectId { get; set; }
        public ProjectStatusEnum Status { get; set; }

        public int AuditReasonId { get; set; }

        public string ReasonOth { get; set; }
    }
}
