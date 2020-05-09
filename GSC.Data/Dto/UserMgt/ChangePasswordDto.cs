using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.UserMgt
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "User Name is required.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Old Password is required.")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New Password is required.")]
        public string NewPassword { get; set; }
    }
}