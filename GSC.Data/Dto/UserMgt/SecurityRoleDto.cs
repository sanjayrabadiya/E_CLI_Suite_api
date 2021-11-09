using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Shared.DocumentService;

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
        public FileModel FileModel { get; set; }
        public string RoleIcon { get; set; }
    }

    public class SecurityRoleGridDto : BaseAuditDto
    {
        public string RoleShortName { get; set; }
        public string RoleName { get; set; }
        public bool IsSystemRole { get; set; }
    }
}