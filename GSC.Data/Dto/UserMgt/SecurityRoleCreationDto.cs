using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.UserMgt
{
    public class SecurityRoleCreationDto
    {
        [Required(ErrorMessage = "Role short name is required.")]
        public string RoleShortName { get; set; }

        [Required(ErrorMessage = "Role name is required.")]
        public string RoleName { get; set; }

        public bool IsSystemRole { get; set; }
    }

    public class SecurityRoleUpdationDto : SecurityRoleCreationDto
    {
        [Required(ErrorMessage = "Id is required.")]
        public int Id { get; set; }
    }
}