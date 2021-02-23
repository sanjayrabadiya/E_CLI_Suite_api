using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Hubs;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.UserMgt;
using GSC.Respository.Configuration;
using GSC.Respository.LogReport;
using GSC.Respository.UserMgt;
using GSC.Shared.Configuration;
using GSC.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    public class LoginController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserLoginReportRespository _userLoginReportRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly IConfiguration _configuration;
        private readonly IOptions<EnvironmentSetting> _environmentSetting;
        private readonly ICentreUserService _centreUserService;
        private readonly IMapper _mapper;
        private readonly ILoginPreferenceRepository _loginPreferenceRepository;
        private readonly IReleaseSettingRepository _releaseSettingRepository;

        public LoginController(
            IUserRoleRepository userRoleRepository,
            IUserRepository userRepository,
            IUserLoginReportRespository userLoginReportRepository,
            IUnitOfWork uow,
            IHubContext<MessageHub> hubContext,
            IConfiguration configuration,
            IOptions<EnvironmentSetting> environmentSetting,
            ICentreUserService centreUserService, IMapper mapper,
            ILoginPreferenceRepository loginPreferenceRepository,
            IReleaseSettingRepository releaseSettingRepository)
        {
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
            _uow = uow;
            _userLoginReportRepository = userLoginReportRepository;
            _hubContext = hubContext;
            _configuration = configuration;
            _environmentSetting = environmentSetting;
            _centreUserService = centreUserService;
            _mapper = mapper;
            _loginPreferenceRepository = loginPreferenceRepository;
            _releaseSettingRepository = releaseSettingRepository;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new UserViewModel();

            if (_environmentSetting.Value.IsPremise)
                user = _userRepository.ValidateUser(dto.UserName, dto.Password);
            else
                user = await _centreUserService.ValidateClient(dto);

            if (!_environmentSetting.Value.IsPremise)
            {
                var company = _loginPreferenceRepository.All.Where(x => x.CompanyId == user.CompanyId).FirstOrDefault();
                if (user.FailedLoginAttempts > company.MaxLoginAttempt)
                {
                    var users = _userRepository.Find(user.UserId);
                    users.IsLocked = true;
                    _userRepository.Update(users);
                    _uow.Save();
                }
            }

            if (!user.IsValid)
            {
                ModelState.AddModelError("UserName", user.ValidateMessage);
                return BadRequest(ModelState);
            }

            var roles = _userRoleRepository.GetRoleByUserName(dto.UserName);

            if (roles.Count <= 0)
            {
                ModelState.AddModelError("UserName",
                    "You have not assigned any role, Please contact your administrator");
                return BadRequest(ModelState);
            }

            var loginUser = await CheckifAlreadyLogin(user.UserId);
            if (loginUser.IsLogin)
            {
                //var errorResult = new ObjectResult(dto.UserName)
                //{
                //    StatusCode = 409
                //};
                //return errorResult;

                return Ok(new { alreadyLoggedIn = true });
            }

            if (roles.Count == 1)
                dto.RoleId = roles.First().Id;

            dto.AskToSelectRole = false;
            if (dto.RoleId == 0)
            {
                dto.Roles = roles;
                dto.AskToSelectRole = true;
                dto.IsFirstTime = user.IsFirstTime;
                return Ok(dto);
            }

            var validatedUser = _userRepository.BuildUserAuthObject(user, dto.RoleId);

            if (!string.IsNullOrEmpty(validatedUser.Token))
            {
                TokenProcess(validatedUser.UserId, validatedUser.RefreshToken);
            }

            _uow.Save();

            return Ok(validatedUser);
        }

        void TokenProcess(int userId, string refreshToken)
        {
            if (_environmentSetting.Value.IsPremise)
            {
                _userRepository.UpdateRefreshToken(userId, refreshToken);
            }
            else
            {
                UpdateRefreshTokanDto _refreshtoken = new UpdateRefreshTokanDto();
                _refreshtoken.UserID = userId;
                _refreshtoken.RefreshToken = refreshToken;
                _centreUserService.UpdateRefreshToken(_refreshtoken);
            }

            _uow.Save();
        }

        [Route("MobileLogIn")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginMobile([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var user = new UserViewModel();
            //var user = _userRepository.ValidateUser(dto.UserName, dto.Password);
            if (_environmentSetting.Value.IsPremise)
                user = _userRepository.ValidateUser(dto.UserName, dto.Password);
            else
                user = await _centreUserService.ValidateClient(dto);


            if (!user.IsValid)
            {
                ModelState.AddModelError("UserName", user.ValidateMessage);
                return BadRequest(ModelState);
            }

            var roles = _userRoleRepository.GetRoleByUserName(dto.UserName);

            if (roles.Count <= 0)
            {
                ModelState.AddModelError("UserName",
                    "You have not assigned any role, Please contact your administrator");
                return BadRequest(ModelState);
            }

            var loginUser = await CheckifAlreadyLogin(user.UserId);
            if (loginUser.IsLogin)
            {
                //var errorResult = new ObjectResult(dto.UserName)
                //{
                //    StatusCode = 409
                //};
                //return errorResult;

                return Ok(new { alreadyLoggedIn = true });
            }

            if (roles.Count == 1)
                dto.RoleId = roles.First().Id;

            dto.AskToSelectRole = false;
            if (dto.RoleId == 0)
            {
                dto.Roles = roles;
                dto.AskToSelectRole = true;
                dto.IsFirstTime = user.IsFirstTime;
                return Ok(dto);
            }


            var validatedUser = _userRepository.BuildUserAuthObject(user, dto.RoleId);
            validatedUser.alreadyLoggedIn = false;
            if (!string.IsNullOrEmpty(validatedUser.Token))
            {
                _userRepository.UpdateRefreshToken(validatedUser.UserId, validatedUser.RefreshToken);
            }

            _uow.Save();

            return Ok(validatedUser);

        }

        [HttpPost]
        [Route("role")]
        public async Task<IActionResult> ValidateLoginWithRole([FromBody] LoginRoleDto loginDto)
        {
            Data.Entities.UserMgt.User user;
            if (loginDto.UserId > 0)
                user = _userRepository.Find(loginDto.UserId);
            else
                user = _userRepository.FindBy(x =>
                        x.UserName == loginDto.UserName && x.RoleTokenId == loginDto.Guid && x.DeletedDate == null)
                    .FirstOrDefault();

            if (user == null)
            {
                ModelState.AddModelError("UserName",
                    "You have not assigned any role, Please contact your administrator");
                return BadRequest(ModelState);
            }

            var userViewModel = new UserViewModel();
            userViewModel.IsFirstTime = user.IsFirstTime;
            userViewModel.UserId = user.Id;
            userViewModel.Language = user.Language;
            userViewModel.IsValid = true;
            var validatedUser = _userRepository.BuildUserAuthObject(userViewModel, loginDto.RoleId);

            TokenProcess(validatedUser.UserId, validatedUser.RefreshToken);

            await _hubContext.Clients.All.SendAsync("roleChanged", new { userId = user.Id, roleId = loginDto.RoleId });

            return Ok(validatedUser);
        }

        [HttpGet]
        [Route("getRoleByUserName/{userName}")]
        [AllowAnonymous]
        public IActionResult GetRoleByUserName(string userName)
        {
            return Ok(_userRoleRepository.GetRoleByUserName(userName));
        }

        [HttpGet]
        [Route("MobileLogout/{userId}/{loginReportId}")]
        public IActionResult MobileLogout(int userId, int loginReportId)
        {
            var user = _userRepository.Find(userId);
            if (user == null)
                return NotFound();
            var userLoginReport = _userLoginReportRepository.Find(loginReportId);

            if (userLoginReport == null)
                return NotFound();

            user.IsLogin = false;
            _userRepository.Update(user);

            userLoginReport.LogoutTime = DateTime.Now;
            _userLoginReportRepository.Update(userLoginReport);

            _uow.Save();

            return Ok("Your Session is expired");
        }


        [HttpGet]
        [Route("logout/{userId}/{loginReportId}")]
        public IActionResult Logout(int userId, int loginReportId)
        {
            if (!_environmentSetting.Value.IsPremise)
            {
                _centreUserService.Logout($"{ _environmentSetting.Value.CentralApi}Login/Logout/{userId}/{loginReportId}");
                //  return Ok();
            }

            var user = _userRepository.Find(userId);
            if (user == null)
                return NotFound();
            var userLoginReport = _userLoginReportRepository.Find(loginReportId);

            if (userLoginReport == null)
                return NotFound();

            user.IsLogin = false;
            _userRepository.Update(user);

            userLoginReport.LogoutTime = DateTime.Now;
            _userLoginReportRepository.Update(userLoginReport);

            _uow.Save();

            return Ok();

        }


        [HttpGet]
        [Route("logOutFromEveryWhere/{userName}")]
        [AllowAnonymous]
        public async Task<IActionResult> LogOutFromEveryWhere(string userName)
        {
            bool IsPremise = _environmentSetting.Value.IsPremise;
            if (!IsPremise)
            {
                var result = new UserViewModel();
                result = await _centreUserService.LogOutFromEveryWhere($"{_environmentSetting.Value.CentralApi}Login/LogOutFromEveryWhere/{userName}");
                if (result != null)
                {
                    string companyCode = $"CompanyId{result.CompanyId}";
                    _userLoginReportRepository.SetDbConnection(result.ConnectionString);

                    var userLoginReports = _userLoginReportRepository.FindBy(t => t.UserId == result.UserId && t.LogoutTime == null).ToList();
                    userLoginReports.ForEach(t =>
                    {
                        t.LogoutTime = DateTime.Now;
                        _userLoginReportRepository.Update(t);
                    });

                    var user = _userRepository.All.Where(x => x.Id == result.UserId && x.DeletedDate == null).FirstOrDefault();
                    user.IsLogin = false;
                    _userRepository.Update(user);

                    _uow.Save();

                    await _hubContext.Clients.All.SendAsync("logofffromeverywhere", user.Id);
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                var users = _userRepository.All.FirstOrDefault(x => x.UserName == userName && x.DeletedDate == null);

                if (users != null)
                {
                    await _hubContext.Clients.All.SendAsync("logofffromeverywhere", users.Id);
                    users.IsLogin = false;
                    _userRepository.Update(users);
                    _uow.Save();
                }
                else
                {
                    return NotFound();
                }

                return Ok();
            }
        }

        private async Task<Data.Entities.UserMgt.User> CheckifAlreadyLogin(int userId)
        {
            return await _userRepository.FindAsync(userId);
        }

        [HttpPost]
        [Route("Refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto token)
        {
            return Ok(await _userRepository.Refresh(token.AccessToken, token.RefreshToken));
        }


        [HttpPost]
        [Route("ValidateUserForSignature")]
        public async Task<IActionResult> ValidateUserForSignature([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var user = new UserViewModel();
            if (_environmentSetting.Value.IsPremise)
                user = _userRepository.ValidateUser(dto.UserName, dto.Password);
            else
                user = await _centreUserService.ValidateClient(dto);
            if (!user.IsValid)
            {
                ModelState.AddModelError("UserName", user.ValidateMessage);
                return BadRequest(ModelState);
            }

            return Ok(user);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("VersionNum")]
        public ActionResult VersionNum()
        {
            var VersionNum = _releaseSettingRepository.GetVersionNum();
            return Ok(VersionNum);
                
        }
    }
}