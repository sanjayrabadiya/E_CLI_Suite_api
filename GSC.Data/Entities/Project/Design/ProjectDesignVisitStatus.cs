using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesignVisitStatus : BaseEntity, ICommonAduit
    {
        public int ProjectDesignVisitId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public ScreeningVisitStatus VisitStatusId { get; set; }
        public int? CompanyId { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
    }
}
