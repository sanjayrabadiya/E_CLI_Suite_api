using GSC.Data.Dto.UserMgt;
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
        Task<RefreshTokenDto> Refresh(RefreshTokenDto tokenn);
        void DeleteUser(string clientUrl, int Id);
        Task<CommonResponceView> ChangePassword(ChangePasswordDto loginDto, string clientUrl);
        Task<CommonResponceView> ActiveUser(string clientUrl, int Id);
    }
}
