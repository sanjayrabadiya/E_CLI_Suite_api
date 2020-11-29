﻿
using GSC.Shared.Generic;

namespace GSC.Shared.Security
{
    public class UserViewModel
    {
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public string ValidateMessage { get; set; }
        public bool IsValid { get; set; }
        public bool IsFirstTime { get; set; }
        public PrefLanguage? Language { get; set; }
        public int FailedLoginAttempts { get; set; }

    }
}
