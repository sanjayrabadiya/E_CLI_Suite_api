using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Project.Rights
{
    public class ProjectModuleRights: BaseEntity, ICommonAduit
    {
        public int ProjectID { get; set; }
        public int AppScreenID { get; set; }

    }
}
