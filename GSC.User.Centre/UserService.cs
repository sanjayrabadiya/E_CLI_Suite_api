using GSC.Respository.Configuration;
using GSC.Shared.Generic;
using GSC.Shared.Security;
using System.Net.Http;
using System.Threading.Tasks;

namespace GSC.User.Centre
{
    public class UserService : IUserService
    {
        private readonly ILoginPreferenceRepository _loginPreferenceRepository;
        private readonly HttpClient _httpClient;

      
        public UserService(ILoginPreferenceRepository loginPreferenceRepository, HttpClient httpClient)
        {
            _loginPreferenceRepository = loginPreferenceRepository;
            _httpClient = httpClient;
        }
        public async Task<UserViewModel> ValidateClient(string userName, string password, string clientUrl)
        {
            var result = await HttpService.Post<UserViewModel>(_httpClient, clientUrl + "/Login", new LoginViewModel { Password = password, UserName = userName });
            //if (result != null && user.FailedLoginAttempts > result.MaxLoginAttempt)
            //{
            //    user.IsLocked = true;
            //    Update(user);
            //    _context.Save();
            //}

            return result;
        }
    }
}
