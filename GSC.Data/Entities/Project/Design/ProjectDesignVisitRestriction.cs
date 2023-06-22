using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesignVisitRestriction : BaseEntity, ICommonAduit
    {
        public int SecurityRoleId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public bool IsAdd { get; set; }
        public SecurityRole SecurityRole { get; set; }
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
    }
}
