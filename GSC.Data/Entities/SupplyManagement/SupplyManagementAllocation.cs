using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementAllocation : BaseEntity
    {
        public int ProjectId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }

        public string Value { get; set; }

        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }

        public ProjectDesignVisit ProjectDesignVisit { get; set; }
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }

    }
}
