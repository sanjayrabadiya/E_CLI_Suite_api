using GSC.Shared.Generic;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.User.Centre
{
    public class UserViewModel
    {
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public string ValidateMessage { get; set; }
        public bool IsValid { get; set; }
        public bool IsFirstTime { get; set; }
        public PrefLanguage? Language { get; set; }

    }
}
