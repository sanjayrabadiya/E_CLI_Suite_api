using GSC.Data.Entities.Common;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.CTMS
{
    public class StudyPlanTaskResourceDto : BaseDto
    {
        public int StudyPlanTaskId { get; set; }
        public int SecurityRoleId { get; set; }
        public int UserId { get; set; }
        public int[] Users { get; set; }
        public StudyPlanTask StudyPlanTask { get; set; }
        public SecurityRole SecurityRole { get; set; }
        public User User { get; set; }
        public string RoleName { get; set; }
        public string UserName { get; set; }
    }


    public class StudyPlanTaskResourceGridDto : BaseAuditDto
    {
        public int SecurityRoleId { get; set; }
        public int UserId { get; set; }
        public string RoleName { get; set; }
        public string UserName { get; set; }
    }

}
