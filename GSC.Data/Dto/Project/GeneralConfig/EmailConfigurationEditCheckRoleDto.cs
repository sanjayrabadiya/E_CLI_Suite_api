using GSC.Data.Entities.Common;
using System.Collections.Generic;


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
