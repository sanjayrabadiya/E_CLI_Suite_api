using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Project.Design
{
    public class TemplatePermission : BaseEntity, ICommonAduit
    {
        public int SecurityRoleId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public bool IsAdd { get; set; }
        public bool IsEdit { get; set; }
        public SecurityRole SecurityRole { get; set; }
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
    }
}
