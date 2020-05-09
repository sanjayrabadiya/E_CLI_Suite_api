using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.UserMgt
{
    public class UserRoleDto : BaseDto
    {
        [Required(ErrorMessage = "User Name is required.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Role Name is required.")]
        public int UserRoleId { get; set; }
    }
}