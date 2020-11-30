using GSC.Data.Dto.UserMgt;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Shared.Extension;
using GSC.Shared.Generic;
using GSC.Shared.Security;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.UserMgt
{
    public class CentreUserService : ICentreUserService
    {
        private readonly ILoginPreferenceRepository _loginPreferenceRepository;
        private readonly HttpClient _httpClient;
        private readonly IGSCContextExtension _gSCContextExtension;

        public CentreUserService(ILoginPreferenceRepository loginPreferenceRepository,
            HttpClient httpClient, IGSCContextExtension gSCContextExtension)
        {
            _loginPreferenceRepository = loginPreferenceRepository;
            _httpClient = httpClient;
            _gSCContextExtension = gSCContextExtension;
        }
        public async Task<UserViewModel> ValidateClient(LoginDto loginDto, string clientUrl)
        {
            var result = await HttpService.Post<UserViewModel>(_httpClient, clientUrl + "Login/ValidateUser", loginDto);
            if (result != null && result.IsValid)
            {
                _gSCContextExtension.ConfigureServices(result.ConnectionString);
                //if (result != null && result.FailedLoginAttempts > result.MaxLoginAttempt)
                //{
                //    user.IsLocked = true;
                //    Update(user);
                //    _context.Save();
                //}


            }
            return result;
        }
    }
}
