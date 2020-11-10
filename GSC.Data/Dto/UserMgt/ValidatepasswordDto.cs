using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.UserMgt
{
   public class ValidatepasswordDto
    {
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
        [Required(ErrorMessage = "User ID is required.")]
        public int UserID { get; set; }
    }

    public class RefreshTokanDto
    {

        [Required(ErrorMessage = "User ID is required.")]
        public int UserId { get; set; }
        [Required(ErrorMessage = "Refresh Tokan is required.")]
        public string RefreshToken { get; set; }
    }
}
