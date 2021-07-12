using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.UserMgt;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserSettingController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserSettingRepository _userSettingRepository;
        public UserSettingController(
            IUserSettingRepository userSettingRepository,
            IJwtTokenAccesser jwtTokenAccesser,
        IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _userSettingRepository = userSettingRepository;
        }

        // Get project default data
        [HttpGet]
        [Route("GetProjectDefaultData")]
        public IActionResult GetProjectDefaultData()
        {
            return Ok(_userSettingRepository.GetProjectDefaultData());
        }    

        // Get project default data
        [HttpPost]
        public IActionResult Post([FromBody] int projectId)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var exists = _userSettingRepository.All.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.RoleId==_jwtTokenAccesser.RoleId && x.DeletedDate == null).FirstOrDefault();
            if (exists == null)
            {
                UserSetting userSetting = new UserSetting();
                userSetting.ProjectId = projectId;
                userSetting.RoleId = _jwtTokenAccesser.RoleId;
                userSetting.UserId = _jwtTokenAccesser.UserId;

                _userSettingRepository.Add(userSetting);
            }
            else
            {
                exists.ProjectId = projectId;
                _userSettingRepository.Update(exists);
            }
            if (_uow.Save() <= 0) throw new Exception("Set default project failed.");
            return Ok();
        }
    }
}
