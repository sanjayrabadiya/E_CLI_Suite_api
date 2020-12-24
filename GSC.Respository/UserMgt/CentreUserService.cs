using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Respository.Configuration;
using GSC.Respository.LogReport;
using GSC.Shared.Caching;
using GSC.Shared.Configuration;
using GSC.Shared.Extension;
using GSC.Shared.Generic;
using GSC.Shared.Security;
using Microsoft.Extensions.Options;
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
        private readonly IGSCCaching _gSCCaching;
        private readonly IOptions<EnvironmentSetting> _environmentSetting;
        private readonly IUserLoginReportRespository _userLoginReportRepository;
        public CentreUserService(ILoginPreferenceRepository loginPreferenceRepository,
            HttpClient httpClient,
            IGSCCaching gSCCaching, IUserLoginReportRespository userLoginReportRepository,
            IOptions<EnvironmentSetting> environmentSetting)
        {
            _loginPreferenceRepository = loginPreferenceRepository;
            _httpClient = httpClient;
            _gSCCaching = gSCCaching;
            _userLoginReportRepository = userLoginReportRepository;
            _environmentSetting = environmentSetting;
        }
        public async Task<UserViewModel> ValidateClient(LoginDto loginDto)
        {
            var result = await HttpService.Post<UserViewModel>(_httpClient, $"{_environmentSetting.Value.CentralApi}Login/ValidateUser", loginDto);
            if (result != null)
            {
                string companyCode = $"CompanyId{result.CompanyId}";
                _userLoginReportRepository.SetDbConnection(result.ConnectionString);
                if (result.IsValid)
                {
                    await HttpService.Post<UserViewModel>(_httpClient, $"{_environmentSetting.Value.CentralApi}Login/LockStatus/{result.UserId}/{false}", "");
                    _gSCCaching.Remove(companyCode);
                    _gSCCaching.Add(companyCode, result.ConnectionString, DateTime.Now.AddDays(7));
                }
                else
                {
                    var company = _loginPreferenceRepository.All.Where(x => x.CompanyId == result.CompanyId).FirstOrDefault();
                    if (result.FailedLoginAttempts > company.MaxLoginAttempt)
                    {
                        result.ValidateMessage = "User is locked, Please contact your administrator";
                        await HttpService.Post<UserViewModel>(_httpClient, $"{_environmentSetting.Value.CentralApi}Login/LockStatus/{result.UserId}/{true}", "");
                    }
                    _userLoginReportRepository.SaveLog(result.ValidateMessage, result.UserId, loginDto.UserName);
                }
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
    }
}
