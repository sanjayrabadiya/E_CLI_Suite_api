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


        public async Task<RefreshToken> RefreshToken(RefreshTokenDto tokenn)
        {
            var result = await HttpService.Post<RefreshToken>(_httpClient, $"{_environmentSetting.Value.CentralApi}Login/RefreshToken", tokenn);
            return result;
        }

        public async Task<CommonResponceView> SaveUser(UserDto userDto, string clientUrl)
        {
            var result = await HttpService.Post<CommonResponceView>(_httpClient, clientUrl + "User", userDto);
            return result;
        }

        public async Task<CommonResponceView> UpdateUser(UserDto userDto, string clientUrl)
        {
            var result = await HttpService.Put<CommonResponceView>(_httpClient, clientUrl + "User", userDto);
            return result;
        }

        public async void DeleteUser(string clientUrl, int Id)
        {
            await HttpService.Delete(_httpClient, clientUrl + "User/" + Id);
        }
        public async Task<CommonResponceView> ChangePassword(ChangePasswordDto loginDto, string clientUrl)
        {
            var result = await HttpService.Post<CommonResponceView>(_httpClient, clientUrl + "User/ChangePassword", loginDto);
            return result;
        }

        public async Task<CommonResponceView> ActiveUser(string clientUrl, int Id)
        {
            var result = await HttpService.Get<CommonResponceView>(_httpClient, clientUrl + "User/Active/" + Id);
            return result;
        }

        public async Task<UserViewModel> GetUserDetails(string clientUrl)
        {
            var result = await HttpService.Get<UserViewModel>(_httpClient, clientUrl);
            return result;
        }

        public async Task<User> GetUserData(string clientUrl)
        {
            var result = await HttpService.Get<User>(_httpClient, clientUrl);
            return result;
        }

        public async Task<UserViewModel> LogOutFromEveryWhere(string clientUrl)
        {
            var result = await HttpService.Get<UserViewModel>(_httpClient, clientUrl);
            return result;
        }
        public async void UpdateRefreshToken(UpdateRefreshTokanDto refreshTokanDto)
        {
            await HttpService.Post(_httpClient, $"{_environmentSetting.Value.CentralApi}Login/UpdateRefreshToken", refreshTokanDto);
        }

        public async Task<string> InsertOtpCenteral(string clientUrl)
        {
            string result = await HttpService.Get(_httpClient, clientUrl, null);
            return result;
        }

        public async Task<string> VerifyOtpCenteral(string clientUrl, UserOtpDto userOtpDto)
        {
            string result = await HttpService.Post(_httpClient, clientUrl, userOtpDto);
            return result;
        }
        public async Task<string> ChangePasswordByOtpCenteral(string clientUrl, UserOtpDto userOtpDto)
        {
            string result = await HttpService.Post(_httpClient, clientUrl, userOtpDto);
            return result;
        }
        public async Task<UserOtp> GetUserOtpDetails(string clientUrl)
        {
            var result = await HttpService.Get<UserOtp>(_httpClient, clientUrl);
            return result;
        }

        public async Task Logout(string clientUrl)
        {
            var result = await HttpService.Get<UserOtp>(_httpClient, clientUrl);
        }

        public async Task GetBlockedUser(string clientUrl)
        {
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
            var result = await HttpService.Get<Companystudyconfig>(_httpClient, clientUrl);
            return result;
        }
        public async Task<UserViewModel> ValidateClientData(LoginDto loginDto)
        {
            var result = await HttpService.Post<UserViewModel>(_httpClient, $"{_environmentSetting.Value.CentralApi}Login/ValidateUser", loginDto);

            return result;
        }
    }
}
