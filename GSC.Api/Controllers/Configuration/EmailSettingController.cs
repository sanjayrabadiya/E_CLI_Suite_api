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
using GSC.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Configuration
{
    [Route("api/[controller]")]
    public class EmailSettingController : BaseController
    {
        private readonly IEmailSettingRepository _emailSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public EmailSettingController(IEmailSettingRepository emailSettingRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _emailSettingRepository = emailSettingRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var emailSettings = _emailSettingRepository.All.Where(x =>
                x.CompanyId == _jwtTokenAccesser.CompanyId
                && isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).OrderByDescending(x => x.Id).ToList();
            var emailSettingsDto = _mapper.Map<IEnumerable<EmailSettingDto>>(emailSettings);
            return Ok(emailSettingsDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var emailSetting = _emailSettingRepository.Find(id);
            var emailSettingDto = _mapper.Map<EmailSettingDto>(emailSetting);
            return Ok(emailSettingDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] EmailSettingDto emailSettingDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            emailSettingDto.Id = 0;
            var emailSetting = _mapper.Map<EmailSetting>(emailSettingDto);
            _emailSettingRepository.Add(emailSetting);
            if (_uow.Save() <= 0) throw new Exception("Creating Email Setting failed on save.");
            return Ok(emailSetting.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] EmailSettingDto emailSettingDto)
        {
            if (emailSettingDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var emailSetting = _mapper.Map<EmailSetting>(emailSettingDto);

            _emailSettingRepository.Update(emailSetting);
            if (_uow.Save() <= 0) throw new Exception("Updating Email Setting failed on save.");
            return Ok(emailSetting.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _emailSettingRepository.Find(id);

            if (record == null)
                return NotFound();

            _emailSettingRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _emailSettingRepository.Find(id);

            if (record == null)
                return NotFound();
            _emailSettingRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetEmailFromDropDown")]
        public IActionResult GetEmailFromDropDown()
        {
            return Ok(_emailSettingRepository.GetEmailFromDropDown());
        }
    }
}