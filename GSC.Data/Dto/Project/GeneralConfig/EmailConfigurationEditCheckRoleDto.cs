using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Project.Generalconfig
{
    public class EmailConfigurationEditCheckRoleDto : BaseDto
    {
        public int EmailConfigurationEditCheckId { get; set; }
        public string Subject { get; set; }
        public string EmailBody { get; set; }
        public List<int> RoleId { get; set; }

        public bool IsSMS { get; set; }

    }
}
