using GSC.Data.Dto.UserMgt;
using GSC.Respository.Configuration;
using GSC.Shared.Caching;
using GSC.Shared.Extension;
using GSC.Shared.Generic;
using GSC.Shared.Security;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GSC.Respository.UserMgt
{
    public class CentreUserService : ICentreUserService
    {
        private readonly ILoginPreferenceRepository _loginPreferenceRepository;
        private readonly HttpClient _httpClient;
        private readonly IGSCContextExtension _gSCContextExtension;
        private readonly IGSCCaching _gSCCaching;
        private readonly IUserRepository _userRepository;
        public CentreUserService(ILoginPreferenceRepository loginPreferenceRepository,
            HttpClient httpClient, IGSCContextExtension gSCContextExtension,
            IGSCCaching gSCCaching, IUserRepository userRepository)
        {
            _loginPreferenceRepository = loginPreferenceRepository;
            _httpClient = httpClient;
            _gSCContextExtension = gSCContextExtension;
            _gSCCaching = gSCCaching;
            _userRepository = userRepository;
        }
        public async Task<UserViewModel> ValidateClient(LoginDto loginDto, string clientUrl)
        {
            var result = await HttpService.Post<UserViewModel>(_httpClient, clientUrl + "Login/ValidateUser", loginDto);
            if (result != null)
            {
                string companyCode = $"CompanyId{result.CompanyId}";
                var user = _userRepository.Find(result.UserId);
                if (result.IsValid)
                {
                    user.IsLocked = false;
                    user.FailedLoginAttempts = 0;
                    await HttpService.Post<UserViewModel>(_httpClient, clientUrl + $"Login/LockStatus/{result.UserId}/{false}", "");
                    _gSCContextExtension.ConfigureServices(result.ConnectionString);
                    _gSCCaching.Remove(companyCode);
                    _gSCCaching.Add(companyCode, result.ConnectionString, DateTime.Now.AddDays(7));
                }
                else
                {
                    var company = _loginPreferenceRepository.All.Where(x => x.CompanyId == result.CompanyId).FirstOrDefault();
                    if (result.FailedLoginAttempts > company.MaxLoginAttempt)
                    {
                        result.ValidateMessage = "User is locked, Please contact your administrator";
                        user.IsLocked = true;
                        await HttpService.Post<UserViewModel>(_httpClient, clientUrl + $"Login/LockStatus/{result.UserId}/{true}", "");
                    }

                }
                _userRepository.Update(user);
            }
            return result;
        }
    }
}
