using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesignVisitStatus : BaseEntity
    {
        public int ProjectDesignVisitId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public int VisitStatusId { get; set; }
        public int? CompanyId { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
    }
}
