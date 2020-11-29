using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using GSC.Shared.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.LogReport;
using GSC.Respository.UserMgt;
using GSC.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using GSC.Shared.Configuration;
using GSC.Shared.Generic;

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
        private readonly IRoleRepository _roleRepository;
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IConfiguration _configuration;
        private readonly IAPICall _centeralApi;

        public LoginController(
            IUserRoleRepository userRoleRepository,
            IUserRepository userRepository,
            IOptions<JwtSettings> settings,
            IRoleRepository roleRepository,
            IUserLoginReportRespository userLoginReportRepository,
            IUnitOfWork uow,
            ICompanyRepository companyRepository,
            IUploadSettingRepository uploadSettingRepositor,
            IHubContext<Notification> notificationHubContext,
            IAppSettingRepository appSettingRepository,
            IRolePermissionRepository rolePermissionRepository,
            IConfiguration configuration, IAPICall centerlApi, IMapper mapper)
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
            _roleRepository = roleRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _configuration = configuration;
            _centeralApi = centerlApi;
            _mapper = mapper;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _userRepository.ValidateUser(dto.UserName, dto.Password);
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
                _userRepository.UpdateRefreshToken(validatedUser.UserId, validatedUser.RefreshToken);
            }

            _uow.Save();

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
                _userRepository.UpdateRefreshToken(validatedUser.UserId, validatedUser.RefreshToken);
            }

            _uow.Save();

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

            var userViewModel = new UserViewModel();
            userViewModel.IsFirstTime = user.IsFirstTime;
            userViewModel.UserId = user.Id;
            userViewModel.Language = user.Language;
            userViewModel.IsValid = true;
            var validatedUser = _userRepository.BuildUserAuthObject(userViewModel, loginDto.RoleId);

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
            if (Convert.ToBoolean(_configuration["IsCloud"]))
                _centeralApi.Post(token, $"{_configuration["EndPointURL"]}/Login/Refresh");
            //_centeralRepository.Refresh(token.AccessToken, token.RefreshToken);
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