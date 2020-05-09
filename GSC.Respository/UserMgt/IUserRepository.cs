using System.Collections.Generic;
using System.Security.Claims;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;

namespace GSC.Respository.UserMgt
{
    public interface IUserRepository : IGenericRepository<User>
    {
        User ValidateUser(string userName, string password);
        string DuplicateUserName(User objSave);
        void UpdateUserStatus(int id);
        List<DropDownDto> GetUserName();
        RefreshTokenDto Refresh(string accessToken, string refreshToken);
        void UpdateRefreshToken(int userid, string refreshToken);
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        List<UserDto> GetUsers(bool isDeleted);
        List<DropDownDto> GetUserNameDropdown();
    }
}