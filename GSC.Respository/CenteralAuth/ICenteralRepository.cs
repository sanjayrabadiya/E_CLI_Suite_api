using GSC.Centeral.Models;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using System.Collections.Generic;
using System.Security.Claims;

namespace GSC.Respository.CenteralAuth
{
    public interface ICenteralRepository: IGenericRepository<Users>
    {
        Users CheckValidUser(string userName);
        void UpdateRefreshToken(int userid, string refreshToken);
        RefreshTokenDto Refresh(string accessToken, string refreshToken);
        string DuplicateUserName(Users objSave);
        int Save(Users objSave);
    }
}
