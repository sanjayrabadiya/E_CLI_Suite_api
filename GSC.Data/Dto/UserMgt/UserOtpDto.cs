using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.UserMgt
{
    public class UserOtpDto
    {
        [Required(ErrorMessage = "User Name is required.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Otp is required.")]
        public string Otp { get; set; }

        public string Password { get; set; }
    }
}