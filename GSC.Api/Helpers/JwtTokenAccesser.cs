using System.Linq;
using GSC.Helper;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace GSC.Api.Helpers
{
    public class JwtTokenAccesser : IJwtTokenAccesser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtTokenAccesser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            var user = GetHeader("user");
            if (user != null && user.Any())
            {
                var userInfo = JsonConvert.DeserializeObject<UserInfo>(user);
                UserId = userInfo.UserId;
                UserName = userInfo.UserName;
                CompanyId = userInfo.CompanyId;
                RoleId = userInfo.RoleId;
            }

            IpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }


        public string GetHeader(string key)
        {
            var value = _httpContextAccessor.HttpContext.Request.Headers[key];
            return value;
        }

        public int UserId { get; }

        public string UserName { get; }

        public int CompanyId { get; }

        public int RoleId { get; }

        public string IpAddress { get; }
    }

    public class UserInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int CompanyId { get; set; }
        public int RoleId { get; set; }
    }
}