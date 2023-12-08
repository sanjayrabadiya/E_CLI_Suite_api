using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;
using System.Collections.Generic;

namespace GSC.Data.Entities.CTMS
{
    public class UserAccess : BaseEntity, ICommonAduit
    {
        public int ParentProjectId { get; set; }
        public int ProjectId { get; set; }
        public int UserRoleId { get; set; }
        public bool IsSite {  get; set; }
        public UserRole UserRole { get; set; }
        public Master.Project Project { get; set; }
    }
}
