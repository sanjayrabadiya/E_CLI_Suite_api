using GSC.Shared.Generic;
using System;

namespace GSC.Shared.JWTAuth
{
    public interface IJwtTokenAccesser
    {
        int UserId { get; }
        string UserName { get; }
        int CompanyId { get; }
        int RoleId { get; }
        string RoleName { get; }
        string IpAddress { get; }
        int Language { get; }
        string GetHeader(string key);
        DateTime GetClientDate();
    }
}