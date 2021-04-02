using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.UserMgt
{
    public class UserSetting : BaseEntity, ICommonAduit
    {
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public Data.Entities.Master.Project Project { get; set; }
    }
}
