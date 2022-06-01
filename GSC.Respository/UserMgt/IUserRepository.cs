using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Shared.Security;

namespace GSC.Respository.UserMgt
{
    public interface IUserRepository : IGenericRepository<User>
    {
        UserViewModel ValidateUser(string userName, string password);
        string DuplicateUserName(User objSave);
        void UpdateUserStatus(int id);
        List<DropDownDto> GetUserName();
        Task<RefreshTokenDto> Refresh(string accessToken, string refreshToken);
        void UpdateRefreshToken(int userid, string refreshToken);
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        List<UserGridDto> GetUsers(bool isDeleted);
        List<DropDownDto> GetUserNameDropdown();
        LoginResponseDto BuildUserAuthObject(UserViewModel userViewModel, int roleId);
        void UpdateIsLogin(int id, bool isLogin);
        List<UserGridDto> GetPatients(PatientDto userDto);

        User GetUserById(int id);

       
    }
}