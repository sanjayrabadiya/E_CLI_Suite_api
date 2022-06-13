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
       
        void DeleteUser(string clientUrl, int Id);
       
        Task<CommonResponceView> ActiveUser(string clientUrl, int Id);


        Task<UserViewModel> GetUserDetails(string clientUrl);
        Task<User> GetUserData(string clientUrl);
       
        
        Task<UserOtp> GetUserOtpDetails(string clientUrl);
        Task GetBlockedUser(string clientUrl);
        Task SentConnectionString(int CompanyID, string clientUrl);
        Task<Companystudyconfig> Getnoofstudy(string clientUrl);
    }
}
