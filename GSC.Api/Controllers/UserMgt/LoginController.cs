using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.UserMgt;
using GSC.Respository.Configuration;
using GSC.Respository.LogReport;
using GSC.Respository.UserMgt;
using GSC.Shared.Configuration;
using GSC.Shared.JWTAuth;
using GSC.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;

namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    public class LoginController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserLoginReportRespository _userLoginReportRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IConfiguration _configuration;
        private readonly IOptions<EnvironmentSetting> _environmentSetting;
        private readonly ICentreUserService _centreUserService;
        private readonly IMapper _mapper;
        private readonly ILoginPreferenceRepository _loginPreferenceRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public LoginController(
            IUserRoleRepository userRoleRepository,
            IUserRepository userRepository,
            IUserLoginReportRespository userLoginReportRepository,
            IUnitOfWork uow,
            IConfiguration configuration,
            IOptions<EnvironmentSetting> environmentSetting,
            ICentreUserService centreUserService, IMapper mapper,
            ILoginPreferenceRepository loginPreferenceRepository,
            IJwtTokenAccesser jwtTokenAccesser
            )
        {
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
            _uow = uow;
            _userLoginReportRepository = userLoginReportRepository;
            // _hubContext = hubContext;
            _configuration = configuration;
            _environmentSetting = environmentSetting;
            _centreUserService = centreUserService;
            _mapper = mapper;
            _loginPreferenceRepository = loginPreferenceRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [Route("GetRoles")]
        [HttpPost]
        public async Task<IActionResult> GetRoles()
        {
            LoginDto dto = new LoginDto();
            if (!_environmentSetting.Value.IsPremise)
            {
                var user = await _centreUserService.ValidateClient();
                if (user == null)
                {
                    ModelState.AddModelError("UserName", "User not valid");
                    return BadRequest(ModelState);
                }
            }
         
            var roles = _userRoleRepository.GetRoleByUserId(_jwtTokenAccesser.UserId);

            if (roles.Count <= 0)
            {
                ModelState.AddModelError("UserName",
                    "You have not assigned any role, Please contact your administrator");
                return BadRequest(ModelState);
            }

            if (roles.Count == 1)
            {
                dto.RoleId = roles.First().Id;
                dto.RoleName = roles.First().Value;
            }
            if (roles.Count > 1)
            {
                dto.AskToSelectRole = true;
            }

            dto.Roles = roles;
            return Ok(dto);
        }
        [Route("GetLoginDetails")]
        [HttpGet]
        public IActionResult GetLoginDetails()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validatedUser = _userRepository.GetLoginDetails();

            return Ok(validatedUser);
        }
    }
}