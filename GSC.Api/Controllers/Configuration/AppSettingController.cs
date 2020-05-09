using System;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Configuration
{
    [Route("api/[controller]")]
    public class AppSettingController : BaseController
    {
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUnitOfWork<GscContext> _uow;

        public AppSettingController(IAppSettingRepository appSettingRepository,
            IUnitOfWork<GscContext> uow,
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
            var commonSettiongs = _appSettingRepository.Get<GeneralSettingsDto>(1);
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