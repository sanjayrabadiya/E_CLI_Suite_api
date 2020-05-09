using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.UserMgt
{
    public class LoginRoleDto
    {
        [Required(ErrorMessage = "User Name Required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "User Role Required")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "GUI Required")]
        public string Guid { get; set; }

        public int UserId { get; set; }
    }
}