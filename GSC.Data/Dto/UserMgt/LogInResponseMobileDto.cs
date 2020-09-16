using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.UserMgt
{
   public class LogInResponseMobileDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string UserName { get; set; }
        public bool IsFirstTime { get; set; }
        public int UserId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string UserPicUrl { get; set; }
        public string CompanyLogo { get; set; }
        public string CompanyName { get; set; }
        public string LanguageShortName { get; set; }
    }
}
