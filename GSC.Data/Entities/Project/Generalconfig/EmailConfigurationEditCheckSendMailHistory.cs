using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Project.Generalconfig
{
    public class EmailConfigurationEditCheckSendMailHistory : BaseEntity
    {
        public int EmailConfigurationEditCheckId { get; set; }
        public int RoleId { get; set; }
        public int UserId { get; set; }
        public EmailConfigurationEditCheck EmailConfigurationEditCheck { get; set; }
        public SecurityRole Role { get; set; }
        public User User { get; set; }
    }
}
