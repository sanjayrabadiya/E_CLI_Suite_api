using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Centeral.UnitOfWork;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using GSC.Helper.DocumentService;
using GSC.Respository.CenteralAuth;
using GSC.Respository.Configuration;
using GSC.Respository.LogReport;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;


namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    public class LoginController : BaseController
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IOptions<JwtSettings> _settings;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IUserLoginReportRespository _userLoginReportRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IHubContext<Notification> _notificationHubContext;
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly ICenteralRepository _centeralRepository;

        public LoginController(
            IUserRoleRepository userRoleRepository,
            IUserRepository userRepository,
            IOptions<JwtSettings> settings,
            IUserLoginReportRespository userLoginReportRepository,
            IUnitOfWork uow,
            ICompanyRepository companyRepository,
            IUploadSettingRepository uploadSettingRepositor,
            IHubContext<Notification> notificationHubContext,
            IAppSettingRepository appSettingRepository,
            IRolePermissionRepository rolePermissionRepository,
            ICenteralRepository centeralRepository)
        {
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
            _settings = settings;
            _uow = uow;
            _userLoginReportRepository = userLoginReportRepository;
            _companyRepository = companyRepository;
            _uploadSettingRepository = uploadSettingRepositor;
            _notificationHubContext = notificationHubContext;
            _appSettingRepository = appSettingRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _centeralRepository = centeralRepository;        
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _userRepository.ValidateUser(dto.UserName, dto.Password);
            if (user == null)
            {
                ModelState.AddModelError("UserName", "Invalid username or password");
                return BadRequest(ModelState);
            }

            if (user.IsLocked)
            {
                ModelState.AddModelError("UserName", "User is locked, Please contact your administrator");
                _userLoginReportRepository.SaveLog("User is locked, Please contact your administrator", user.Id,
                    dto.UserName);
                return BadRequest(ModelState);
            }

            if (user.ValidFrom.HasValue && user.ValidFrom.Value > DateTime.Now ||
                user.ValidTo.HasValue && user.ValidTo.Value < DateTime.Now)
            {
                ModelState.AddModelError("UserName", "User not active, Please contact your administrator");
                _userLoginReportRepository.SaveLog("User not active, Please contact your administrator", user.Id,
                    dto.UserName);
                return BadRequest(ModelState);
            }

            var roles = _userRoleRepository.GetRoleByUserName(user.UserName);

            if (roles.Count <= 0)
            {
                ModelState.AddModelError("UserName",
                    "You have not assigned any role, Please contact your administrator");
                return BadRequest(ModelState);
            }

            if (roles.Count == 1)
                dto.RoleId = roles.First().Id;

            dto.AskToSelectRole = false;
            if (dto.RoleId == 0)
            {
                dto.Roles = roles;
                dto.AskToSelectRole = true;
                //dto
                dto.IsFirstTime = user.IsFirstTime;
                return Ok(dto);
            }
            
            //var loginUser = await CheckifAlreadyLogin(user);
            //if (!dto.IsAnotherDevice && loginUser.IsLogin)
            //{
            //    var errorResult = new ObjectResult(dto.UserName)
            //    {
            //        StatusCode = 409
            //    };
            //    return errorResult;
            //}

            var validatedUser = BuildUserAuthObject(user, dto.RoleId);

            return Ok(validatedUser);
        }

        [Route("MobileLogIn")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginMobile([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _userRepository.ValidateUser(dto.UserName, dto.Password);
            if (user == null)
            {
                ModelState.AddModelError("UserName", "Invalid username or password");
                return BadRequest(ModelState);
            }

            if (user.IsLocked)
            {
                ModelState.AddModelError("UserName", "User is locked, Please contact your administrator");
                _userLoginReportRepository.SaveLog("User is locked, Please contact your administrator", user.Id,
                    dto.UserName);
                return BadRequest(ModelState);
            }

            if (user.ValidFrom.HasValue && user.ValidFrom.Value > DateTime.Now ||
                user.ValidTo.HasValue && user.ValidTo.Value < DateTime.Now)
            {
                ModelState.AddModelError("UserName", "User not active, Please contact your administrator");
                _userLoginReportRepository.SaveLog("User not active, Please contact your administrator", user.Id,
                    dto.UserName);
                return BadRequest(ModelState);
            }

            var validatedUser = BuildUserAuthObjectMobile(user);
            return Ok(validatedUser);
        }

        [HttpPost]
        [Route("role")]
        [AllowAnonymous]
        public IActionResult ValidateLoginWithRole([FromBody] LoginRoleDto loginDto)
        {
            User user;
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

            var validatedUser = BuildUserAuthObject(user, loginDto.RoleId);

            return Ok(validatedUser);
        }

        [HttpGet]
        [Route("getRoleByUserName/{userName}")]
        [AllowAnonymous]
        public IActionResult GetRoleByUserName(string userName)
        {
            return Ok(_userRoleRepository.GetRoleByUserName(userName));
        }

        private LogInResponseMobileDto BuildUserAuthObjectMobile(User authUser)
        {
            if (authUser.Language == null)
                authUser.Language = PrefLanguage.en;
            var login = new LogInResponseMobileDto
            {
                UserName = authUser.UserName,
                Token = BuildJwtToken(authUser, 0),
                RefreshToken = _userRepository.GenerateRefreshToken(),
                IsFirstTime = authUser.IsFirstTime,
                UserId = authUser.Id,
                FirstName = authUser.FirstName,
                LastName = authUser.LastName,
                Email = authUser.Email,
                LanguageShortName = authUser.Language.ToString()
            };


            var imageUrl = _uploadSettingRepository
                .FindBy(x => x.CompanyId == authUser.CompanyId && x.DeletedDate == null).FirstOrDefault()?.ImageUrl;

            var company = _companyRepository.Find((int)authUser.CompanyId);
            if (company != null)
            {
                login.CompanyName = company.CompanyName;
                login.CompanyLogo = imageUrl + company.Logo;
                login.UserPicUrl = imageUrl +
                                   (authUser.ProfilePic ?? DocumentService.DefulatProfilePic);
            }

            authUser.FailedLoginAttempts = 0;
            authUser.IsLogin = true;
            authUser.RoleTokenId = null;
            authUser.LastLoginDate = DateTime.Now;
            _userLoginReportRepository.SaveLog("Successfully Login", authUser.Id, authUser.UserName);
            
            if (!string.IsNullOrEmpty(login.Token))
            {
                _userRepository.UpdateRefreshToken(login.UserId, login.RefreshToken);               
                _userRepository.Update(authUser);             
            }

            _uow.Save();
            return login;
        }

        private LoginResponseDto BuildUserAuthObject(User authUser, int roleId)
        {
            var roleTokenId = new Guid().ToString();
            if (authUser.Language == null)
                authUser.Language = PrefLanguage.en;
            var login = new LoginResponseDto
            {
                UserName = authUser.UserName,
                Token = roleId == 0 ? null : BuildJwtToken(authUser, roleId),
                RefreshToken = roleId == 0 ? null : _userRepository.GenerateRefreshToken(),
                ExpiredAfter = DateTime.UtcNow.AddMinutes(_settings.Value.MinutesToExpiration),
                IsFirstTime = authUser.IsFirstTime,
                UserId = authUser.Id,
                RoleTokenId = roleTokenId,
                FirstName = authUser.FirstName,
                LastName = authUser.LastName,
                Email = authUser.Email,
                RoleId = roleId,
                Language = authUser.Language,
                LanguageShortName = authUser.Language.ToString()
            };

            var imageUrl ="";
            //var imageUrl = _uploadSettingRepository
            //    .FindBy(x => x.CompanyId == authUser.CompanyId && x.DeletedDate == null).FirstOrDefault()?.ImageUrl;

            var company = _companyRepository.Find((int)authUser.CompanyId);
            if (company != null)
            {
                login.CompanyName = company.CompanyName;
                login.CompanyLogo = imageUrl + company.Logo;
                //login.UserPicUrl = imageUrl +
                //                   (authUser.ProfilePic ?? DocumentService.DefulatProfilePic);
                login.UserPicUrl = DocumentService.ConvertBase64Image(imageUrl +(authUser.ProfilePic ?? DocumentService.DefulatProfilePic));
            }

            login.GeneralSettings = _appSettingRepository.Get<GeneralSettingsDto>(authUser.CompanyId);
            login.Rights = _rolePermissionRepository.GetByUserId(authUser.Id, roleId);
            login.Roles = _userRoleRepository.GetRoleByUserName(authUser.UserName);
            login.RoleName = login.Roles.FirstOrDefault(t=>t.Id == roleId)?.Value;

            authUser.FailedLoginAttempts = 0;
            authUser.RoleTokenId = roleTokenId;
            if (roleId > 0)
            {
                authUser.IsLogin = true;
                authUser.RoleTokenId = null;
                authUser.LastLoginDate = DateTime.Now;
                login.LoginReportId =
                    _userLoginReportRepository.SaveLog("Successfully Login", authUser.Id, authUser.UserName);
            }

            if (!string.IsNullOrEmpty(login.Token))
            {
                _userRepository.UpdateRefreshToken(login.UserId, login.RefreshToken);
                _userRepository.Update(authUser);
                _centeralRepository.UpdateRefreshToken(login.UserId, login.RefreshToken);
            }

            _uow.Save();
            return login;
        }

        private string BuildJwtToken(User user, int roleId)
        {
            var userInfo = new UserInfo();
            userInfo.UserId = user.Id;
            userInfo.UserName = user.UserName;
            userInfo.CompanyId = (int)user.CompanyId;
            userInfo.RoleId = roleId;
            var claims = new List<Claim> { new Claim("gsc_user_token", userInfo.ToJsonString()) };
            return _userRepository.GenerateAccessToken(claims);
        }

        [HttpGet]
        [Route("logout/{userId}/{loginReportId}")]
        public IActionResult Logout(int userId, int loginReportId)
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

            return Ok();
        }

        [HttpGet]
        [Route("logOutFromEveryWhere/{userName}")]
        [AllowAnonymous]
        public async Task<IActionResult> LogOutFromEveryWhere(string userName)
        {
            var user = _userRepository.All.Where(x => x.UserName == userName && x.DeletedDate == null).FirstOrDefault();

            if (user != null)
            {
                var userLoginReports = _userLoginReportRepository.FindBy(t => t.UserId == user.Id && t.LogoutTime == null).ToList();
                userLoginReports.ForEach(t =>
                {
                    t.LogoutTime = DateTime.Now;
                    _userLoginReportRepository.Update(t);
                });

                user.IsLogin = false;
                _userRepository.Update(user);

                _uow.Save();

                await _notificationHubContext.Clients.All.SendAsync("logofffromeverywhere", user.Id);
            }
            else
            {
                return NotFound();
            }

            return Ok();
        }

        private async Task<User> CheckifAlreadyLogin(User user)
        {
            return await _userRepository.FindAsync(user.Id);
        }

        [HttpPost]
        [Route("Refresh")]
        [AllowAnonymous]
        public IActionResult Refresh([FromBody] RefreshTokenDto token)
        {
            _centeralRepository.Refresh(token.AccessToken, token.RefreshToken);
            return Ok(_userRepository.Refresh(token.AccessToken, token.RefreshToken));
        }

        [HttpPost]
        [Route("ValidateUserForSignature")]
        [AllowAnonymous]
        public IActionResult ValidateUserForSignature([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _userRepository.ValidateUser(dto.UserName, dto.Password);
            if (user == null)
            {
                ModelState.AddModelError("UserName", "Invalid password");
                return BadRequest(ModelState);
            }

            return Ok(user);
        }
    }
}