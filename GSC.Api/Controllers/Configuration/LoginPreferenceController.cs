using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.LogReport;
using GSC.Respository.UserMgt;
using GSC.Shared.Configuration;
using GSC.Shared.JWTAuth;
using GSC.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GSC.Api.Controllers.Configuration
{
    [Route("api/[controller]")]
    public class LoginPreferenceController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly ILoginPreferenceRepository _loginPreferenceRepository;
        private readonly IUserLoginReportRespository _userLoginReportRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IUserRepository _userRepository;
        private readonly IOptions<EnvironmentSetting> _environmentSetting;
        private readonly ICentreUserService _centreUserService;

        public LoginPreferenceController(IUserRepository userRepository,
            ILoginPreferenceRepository loginPreferenceRepository,
            IUserLoginReportRespository userLoginReportRepository,
            IOptions<EnvironmentSetting> environmentSetting,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            ICentreUserService centreUserService)
        {
            _userRepository = userRepository;
            _loginPreferenceRepository = loginPreferenceRepository;
            _userLoginReportRepository = userLoginReportRepository;
            _uow = uow;
            _environmentSetting = environmentSetting;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _centreUserService = centreUserService;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var loginPreferences = _loginPreferenceRepository.All.Where(x =>
                x.CompanyId == _jwtTokenAccesser.CompanyId
                && isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).OrderByDescending(x => x.Id).ToList();
            var loginPreferencesDto = _mapper.Map<IEnumerable<LoginPreferenceDto>>(loginPreferences);
            return Ok(loginPreferencesDto);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var loginPreference = _loginPreferenceRepository.Find(id);
            var loginPreferenceDto = _mapper.Map<LoginPreferenceDto>(loginPreference);
            return Ok(loginPreferenceDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] LoginPreferenceDto loginPreferenceDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            loginPreferenceDto.Id = 0;
            var loginPreference = _mapper.Map<LoginPreference>(loginPreferenceDto);
            _loginPreferenceRepository.Add(loginPreference);
            if (_uow.Save() <= 0) throw new Exception("Creating Upload Setting failed on save.");
            return Ok(loginPreference.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] LoginPreferenceDto loginPreferenceDto)
        {
            if (loginPreferenceDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var loginPreference = _mapper.Map<LoginPreference>(loginPreferenceDto);

            _loginPreferenceRepository.Update(loginPreference);
            if (_uow.Save() <= 0) throw new Exception("Updating Upload Setting failed on save.");
            return Ok(loginPreference.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _loginPreferenceRepository.Find(id);

            if (record == null)
                return NotFound();

            _loginPreferenceRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _loginPreferenceRepository.Find(id);

            if (record == null)
                return NotFound();
            _loginPreferenceRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}