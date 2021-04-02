using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesignVariableEncryptRole : BaseEntity, ICommonAduit
    {
        public int ProjectDesignVariableId { get; set; }
        public int RoleId { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
    }
}
