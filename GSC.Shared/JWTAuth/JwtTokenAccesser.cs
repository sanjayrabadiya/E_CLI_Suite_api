using GSC.Shared.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace GSC.Shared.JWTAuth
{
    public class JwtTokenAccesser : IJwtTokenAccesser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtTokenAccesser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            var user = GetHeader("user");
            if (!string.IsNullOrEmpty(user))
            {
                var userInfo = JsonConvert.DeserializeObject<UserInfo>(user);
                UserId = userInfo.UserId;
                UserName = userInfo.UserName;
                RoleName = userInfo.RoleName;
                Language = userInfo.Language;
                CompanyId = userInfo.CompanyId;
                RoleId = userInfo.RoleId;

            }
            if (_httpContextAccessor != null && _httpContextAccessor.HttpContext != null)
                IpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }


        public string GetHeader(string key)
        {
            string value = "";
            if (_httpContextAccessor != null && _httpContextAccessor.HttpContext != null)
                value = _httpContextAccessor.HttpContext.Request.Headers[key];
            return value;
        }

        public int UserId { get; }

        public string UserName { get; }

        public int CompanyId { get; }
        public int Language { get; }
        public int RoleId { get; }
        public string RoleName { get; }

        public string IpAddress { get; }
    }

    public class UserInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int CompanyId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int Language { get; set; }
    }
}