﻿
using GSC.Shared.Generic;

namespace GSC.Shared.Security
{
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string ConnectionString { get; set; }
        public int? CompanyId { get; set; }
        public string ValidateMessage { get; set; }
        public bool IsValid { get; set; }
        public bool IsFirstTime { get; set; }
        public int? Language { get; set; }
        public int FailedLoginAttempts { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public int MinutesToExpiration { get; set; }

        public string UserName { get; set; }

    }

    public class CommonResponceView
    {
        public int Id { get; set; }       
        public string Message { get; set; }
    }
    public class UserViewModelData
    {
      
        public string ConnectionString { get; set; }
      

    }
}
