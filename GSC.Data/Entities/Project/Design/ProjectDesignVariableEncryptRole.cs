using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesignVariableEncryptRole : BaseEntity, ICommonAduit
    {
        public int ProjectDesignVariableId { get; set; }
        public int RoleId { get; set; }
        [ForeignKey("RoleId")] public SecurityRole SecurityRole { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
    }
}
