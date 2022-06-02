using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Shared.Security;
using System.Threading.Tasks;

namespace GSC.Respository.UserMgt
{
    public interface ICentreUserService
    {
        Task<UserViewModelData> ValidateClient();
        Task<CommonResponceView> SaveUser(UserDto userDto, string clientUrl);
        Task<CommonResponceView> UpdateUser(UserDto userDto, string clientUrl);
        Task<RefreshToken> RefreshToken(RefreshTokenDto tokenn);
        void DeleteUser(string clientUrl, int Id);
        Task<CommonResponceView> ChangePassword(ChangePasswordDto loginDto, string clientUrl);
        Task<CommonResponceView> ActiveUser(string clientUrl, int Id);


        Task<UserViewModel> GetUserDetails(string clientUrl);
        Task<User> GetUserData(string clientUrl);
        Task<UserViewModel> LogOutFromEveryWhere(string clientUrl);
        void UpdateRefreshToken(UpdateRefreshTokanDto refreshTokanDto);
        Task<string> InsertOtpCenteral(string clientUrl);
        Task<string> VerifyOtpCenteral(string clientUrl, UserOtpDto userOtpDto);
        Task<string> ChangePasswordByOtpCenteral(string clientUrl, UserOtpDto userOtpDto);
        Task<UserOtp> GetUserOtpDetails(string clientUrl);
        Task Logout(string clientUrl);
        Task GetBlockedUser(string clientUrl);
        Task SentConnectionString(int CompanyID, string clientUrl);
        Task<Companystudyconfig> Getnoofstudy(string clientUrl);

        Task<UserViewModel> ValidateClientData(LoginDto loginDto);

    }
}
