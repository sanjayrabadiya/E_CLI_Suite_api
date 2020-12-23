using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Shared.Security;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.UserMgt
{
    public interface ICentreUserService
    {
        Task<UserViewModel> ValidateClient(LoginDto loginDto);
        Task<CommonResponceView> SaveUser(UserDto userDto, string clientUrl);
        Task<CommonResponceView> UpdateUser(UserDto userDto, string clientUrl);
        Task<RefreshToken> RefreshToken(RefreshTokenDto tokenn);
        void DeleteUser(string clientUrl, int Id);
        Task<CommonResponceView> ChangePassword(ChangePasswordDto loginDto, string clientUrl);
        Task<CommonResponceView> ActiveUser(string clientUrl, int Id);


        Task<UserViewModel> LogoutEverywhere(string clientUrl);
        void UpdateRefreshToken(UpdateRefreshTokanDto refreshTokanDto);
    }
}
