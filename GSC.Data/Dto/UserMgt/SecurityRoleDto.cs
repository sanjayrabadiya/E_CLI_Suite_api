using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.UserMgt
{
    public class SecurityRoleDto : BaseDto
    {
        [Required(ErrorMessage = "Role short name is required.")]
        public string RoleShortName { get; set; }

        [Required(ErrorMessage = "Role name is required.")]
        public string RoleName { get; set; }

        public bool IsSystemRole { get; set; }
        public int? CompanyId { get; set; }
    }

    public class SecurityRoleGridDto : BaseAuditDto
    {
        public string RoleShortName { get; set; }
        public string RoleName { get; set; }
        public bool IsSystemRole { get; set; }
    }
}