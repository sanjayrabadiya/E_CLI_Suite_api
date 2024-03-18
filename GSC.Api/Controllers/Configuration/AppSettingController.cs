using System;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Respository.Configuration;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Configuration
{
    [Route("api/[controller]")]
    public class AppSettingController : BaseController
    {
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUnitOfWork _uow;

        public AppSettingController(IAppSettingRepository appSettingRepository,
            IUnitOfWork uow,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _appSettingRepository = appSettingRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [AllowAnonymous]
        [HttpGet("GetGeneralSettings")]
        public IActionResult GetGeneralSettings()
        {
            var commonSettiongs = _appSettingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);
            return Ok(commonSettiongs);
        }

        [HttpPost("SaveGeneralSettings")]
        public IActionResult SaveGeneralSettings([FromBody] GeneralSettingsDto commonSettiongs)
        {
            _appSettingRepository.Save(commonSettiongs, _jwtTokenAccesser.CompanyId);
            if (_uow.Save() <= 0) throw new Exception("Creating Common Settings failed on save.");
            return Ok();
        }
    }
}