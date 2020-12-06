using System.Collections.Generic;
using System.Security.Claims;


namespace GSC.Shared.JWTAuth
{
    public interface IGenerateToken
    {
        string RefreshToken();
        string AccessToken(IEnumerable<Claim> claims);
    }
}
