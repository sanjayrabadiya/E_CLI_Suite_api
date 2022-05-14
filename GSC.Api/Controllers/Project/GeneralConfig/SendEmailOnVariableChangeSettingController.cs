﻿using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Generalconfig;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Respository.Project.GeneralConfig;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GSC.Api.Controllers.Project.GeneralConfig
{
    [Route("api/[controller]")]
    public class SendEmailOnVariableChangeSettingController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ISendEmailOnVariableChangeSettingRepository _sendEmailOnVariableChangeSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        // private readonly ISendEmailOnVariableValueRepository _sendEmailOnVariableValueRepository;
        public SendEmailOnVariableChangeSettingController(
            IUnitOfWork uow, IMapper mapper, IJwtTokenAccesser jwtTokenAccesser, ISendEmailOnVariableChangeSettingRepository sendEmailOnVariableChangeSettingRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _sendEmailOnVariableChangeSettingRepository = sendEmailOnVariableChangeSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            //      _sendEmailOnVariableValueRepository = sendEmailOnVariableValueRepository;
        }

        // GET: api/<controller>
        [HttpGet("{projectDesignId}")]
        public IActionResult Get(int projectDesignId)
        {

            var lst = _sendEmailOnVariableChangeSettingRepository.GetList(projectDesignId);
            return Ok(lst);
        }

        //[HttpGet("{id}")]
        //public IActionResult Get(int id)
        //{
        //    var ctmsSettings = _ctmsSettingsRepository.FindBy(x => x.ProjectId == id).FirstOrDefault();
        //    var ctmsSettingsDto = _mapper.Map<CtmsSettingsDto>(ctmsSettings);
        //    return Ok(ctmsSettingsDto);
        //}


        [HttpPost]
        public IActionResult Post([FromBody] SendEmailOnVariableChangeSettingDto sendEmailOnVariableChangeSettingDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            sendEmailOnVariableChangeSettingDto.Id = 0;
            var sendEmailOnVariableChangeSetting = _mapper.Map<SendEmailOnVariableChangeSetting>(sendEmailOnVariableChangeSettingDto);
            var validate = _sendEmailOnVariableChangeSettingRepository.Duplicate(sendEmailOnVariableChangeSetting);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _sendEmailOnVariableChangeSettingRepository.Add(sendEmailOnVariableChangeSetting);
            if (_uow.Save() <= 0) throw new Exception("Creating Send Email On Variable Change Setting settings failed on save.");
            return Ok(sendEmailOnVariableChangeSetting.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] SendEmailOnVariableChangeSettingDto sendEmailOnVariableChangeSettingDto)
        {
            if (sendEmailOnVariableChangeSettingDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var record = _sendEmailOnVariableChangeSettingRepository.Find(sendEmailOnVariableChangeSettingDto.Id);
            //var sendEmailOnVariableChangeSetting = _mapper.Map<SendEmailOnVariableChangeSetting>(sendEmailOnVariableChangeSettingDto);
            record.DeletedBy = _jwtTokenAccesser.UserId;
            record.DeletedDate = _jwtTokenAccesser.GetClientDate();
            record.AuditReasonId = sendEmailOnVariableChangeSettingDto.AuditReasonId;
            record.ReasonOth = sendEmailOnVariableChangeSettingDto.ReasonOth;
            _sendEmailOnVariableChangeSettingRepository.Update(record);

            if (_uow.Save() <= 0) throw new Exception("Update Send Email On Variable Change Setting failed on save.");
            return Ok(record.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _sendEmailOnVariableChangeSettingRepository.Find(id);
            if (record == null)
                return NotFound();
            _sendEmailOnVariableChangeSettingRepository.Delete(record);
            _uow.Save();
            return Ok();
        }
    }
}
