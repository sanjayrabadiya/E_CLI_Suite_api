using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.UserMgt
{
    public class SecurityRole : BaseEntity, ICommonAduit
    {
        public string RoleShortName { get; set; }

        public string RoleName { get; set; }

        public bool IsSystemRole { get; set; }
        public int? CompanyId { get; set; }
        public string RoleIcon { get; set; }
    }
}