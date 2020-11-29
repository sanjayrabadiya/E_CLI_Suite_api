using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using GSC.Shared.Generic;
using System;
using System.Collections.Generic;

namespace GSC.Data.Dto.UserMgt
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiredAfter { get; set; }
        public string UserName { get; set; }
        public int LoginReportId { get; set; }
        public bool IsFirstTime { get; set; }
        public bool PassowordExpired { get; set; }
        public int UserId { get; set; }
        public string RoleTokenId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string UserPicUrl { get; set; }
        public string CompanyLogo { get; set; }
        public string CompanyName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public GeneralSettingsDto GeneralSettings { get; set; }
        public List<AppScreen> Rights { get; set; }
        public IList<DropDownDto> Roles { get; set; }
        public PrefLanguage? Language { get; set; }
        public string LanguageShortName { get; set; }
    }
}