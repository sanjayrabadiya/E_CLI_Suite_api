using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Respository.Configuration;
using GSC.Respository.LogReport;
using GSC.Shared.Caching;
using GSC.Shared.Configuration;
using GSC.Shared.Generic;
using GSC.Shared.JWTAuth;
using GSC.Shared.Security;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GSC.Respository.UserMgt
{
    public class CentreUserService : ICentreUserService
    {
        private readonly ILoginPreferenceRepository _loginPreferenceRepository;
        private readonly HttpClient _httpClient;
        private readonly IGSCCaching _gSCCaching;
        private readonly IOptions<EnvironmentSetting> _environmentSetting;
        private readonly IUserLoginReportRespository _userLoginReportRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public CentreUserService(ILoginPreferenceRepository loginPreferenceRepository,
            HttpClient httpClient,
            IGSCCaching gSCCaching, IUserLoginReportRespository userLoginReportRepository,
            IOptions<EnvironmentSetting> environmentSetting,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _loginPreferenceRepository = loginPreferenceRepository;
            _httpClient = httpClient;
            _gSCCaching = gSCCaching;
            _userLoginReportRepository = userLoginReportRepository;
            _environmentSetting = environmentSetting;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public async Task<UserViewModelData> ValidateClient()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Authorization", _jwtTokenAccesser.GetHeader("Authorization"));

            var result = await HttpService.Get<UserViewModelData>(_httpClient, $"{_environmentSetting.Value.CentralApi}Login/ValidateUserByCompanyId/" + _jwtTokenAccesser.CompanyId);
            if (result != null && !string.IsNullOrEmpty(result.ConnectionString))
            {
                _userLoginReportRepository.SetDbConnection(result.ConnectionString);
                var companyCode = $"CompanyId{_jwtTokenAccesser.CompanyId}";
                _gSCCaching.Remove(companyCode);
                _gSCCaching.Add(companyCode, result.ConnectionString, DateTime.Now.AddDays(7));
            }

            return result;
        }

        public async Task<CommonResponceView> SaveUser(UserDto userDto, string clientUrl)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Authorization", _jwtTokenAccesser.GetHeader("Authorization"));
            var result = await HttpService.Post<CommonResponceView>(_httpClient, clientUrl + "User", userDto);
            return result;
        }

        public async Task<CommonResponceView> UpdateUser(UserDto userDto, string clientUrl)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Authorization", _jwtTokenAccesser.GetHeader("Authorization"));

            var result = await HttpService.Put<CommonResponceView>(_httpClient, clientUrl + "User", userDto);
            return result;
        }

        public async void DeleteUser(string clientUrl, int Id)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Authorization", _jwtTokenAccesser.GetHeader("Authorization"));

            await HttpService.Delete(_httpClient, clientUrl + "User/" + Id);
        }


        public async Task<CommonResponceView> ActiveUser(string clientUrl, int Id)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Authorization", _jwtTokenAccesser.GetHeader("Authorization"));

            var result = await HttpService.Get<CommonResponceView>(_httpClient, clientUrl + "User/Active/" + Id);
            return result;
        }

        public async Task<UserViewModel> GetUserDetails(string clientUrl)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Authorization", _jwtTokenAccesser.GetHeader("Authorization"));

            var result = await HttpService.Get<UserViewModel>(_httpClient, clientUrl);
            return result;
        }

        public async Task<User> GetUserData(string clientUrl)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Authorization", _jwtTokenAccesser.GetHeader("Authorization"));

            var result = await HttpService.Get<User>(_httpClient, clientUrl);
            return result;
        }


        public async Task<UserOtp> GetUserOtpDetails(string clientUrl)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Authorization", _jwtTokenAccesser.GetHeader("Authorization"));

            var result = await HttpService.Get<UserOtp>(_httpClient, clientUrl);
            return result;
        }



        public async Task GetBlockedUser(string clientUrl)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Authorization", _jwtTokenAccesser.GetHeader("Authorization"));

            var result = await HttpService.Get<UserOtp>(_httpClient, clientUrl);
        }

        public async Task SentConnectionString(int CompanyID, string clientUrl)
        {
            string companyCode = $"CompanyId{CompanyID}";
            object connectionStrig;
            _gSCCaching.TryGetValue(companyCode, out connectionStrig);
            if (connectionStrig != null)
            {
                _userLoginReportRepository.SetDbConnection(connectionStrig.ToString());
            }
            else
            {
                var result = await HttpService.Get<CompanyDetailsDto>(_httpClient, clientUrl);
                if (result.CompanyId > 0)
                {
                    companyCode = $"CompanyId{result.CompanyId}";
                    _userLoginReportRepository.SetDbConnection(result.ConnectionString);
                    if (result.ConnectionString != null)
                    {
                        _gSCCaching.Remove(companyCode);
                        _gSCCaching.Add(companyCode, result.ConnectionString, DateTime.Now.AddDays(7));
                    }
                }
            }
        }

        public async Task<Companystudyconfig> Getnoofstudy(string clientUrl)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Authorization", _jwtTokenAccesser.GetHeader("Authorization"));

            var result = await HttpService.Get<Companystudyconfig>(_httpClient, clientUrl);
            return result;
        }
    }
}
