using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.UserMgt
{
    public class SecurityRole : BaseEntity
    {
        public string RoleShortName { get; set; }

        public string RoleName { get; set; }

        public bool IsSystemRole { get; set; }
        public int? CompanyId { get; set; }
    }
}