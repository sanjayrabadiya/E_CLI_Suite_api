﻿using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Schedule;
using GSC.Data.Entities.Project.Schedule;
using GSC.Respository.Project.Schedule;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GSC.Api.Controllers.Project.Schedule
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleTerminateDetailController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IScheduleTerminateDetailRepository _scheduleTerminateDetailRepository;
        private readonly IUnitOfWork _uow;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        

        public ScheduleTerminateDetailController(IMapper mapper, IUnitOfWork uow, IJwtTokenAccesser jwtTokenAccesser, IScheduleTerminateDetailRepository scheduleTerminateDetailRepository)
        {
            _mapper = mapper;
            _scheduleTerminateDetailRepository = scheduleTerminateDetailRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("GetData/{id}")]
        public IActionResult GetData(int id)
        {
            return Ok(_scheduleTerminateDetailRepository.GetDetailById(id));
        }

        [HttpPost]
        public IActionResult Post([FromBody] ScheduleTerminateDetailDto terminateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            terminateDto.Id = 0;
            var terminate = _mapper.Map<ScheduleTerminateDetail>(terminateDto);

            _scheduleTerminateDetailRepository.Add(terminate);
            if (_uow.Save() <= 0) throw new Exception("Creating Schedule Terminate Details failed on save.");
            return Ok(terminate.Id);
        }


        [HttpPut]
        [TransactionRequired]
        public IActionResult Put([FromBody] ScheduleTerminateDetailDto terminateDto)
        {
            if (terminateDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var terminate = _scheduleTerminateDetailRepository.Find(terminateDto.Id);
            terminate.AuditReasonId = terminateDto.AuditReasonId;
            terminate.ReasonOth = terminateDto.ReasonOth;
            terminate.DeletedDate = _jwtTokenAccesser.GetClientDate();
            terminate.DeletedBy = _jwtTokenAccesser.UserId;
            _scheduleTerminateDetailRepository.Update(terminate);
            if (_uow.Save() <= 0) throw new Exception("Updating Schedule Terminate Details failed on save.");

            var terminateAdd = _mapper.Map<ScheduleTerminateDetail>(terminateDto);
            terminateAdd.Id = 0;
            terminateAdd.AuditReasonId = null;
            terminateAdd.ReasonOth = null;
            _scheduleTerminateDetailRepository.Add(terminateAdd);
            if (_uow.Save() <= 0) throw new Exception("Creating Schedule Terminate Details failed on save.");

            return Ok(terminateAdd.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            //var record = _projectScheduleRepository.FindByInclude(x => x.Id == id, x => x.ProjectDesign)
            //    .FirstOrDefault();

            //var recordTemplate = _projectScheduleTemplateRepository.FindByInclude(x => x.ProjectScheduleId == id && x.DeletedDate == null).ToList();

            //if (record == null && recordTemplate == null)
            //    return NotFound();

            //if (!_studyVersionRepository.IsOnTrialByProjectDesing(record.ProjectDesign.Id))
            //{
            //    ModelState.AddModelError("Message", "Can not delete schedule!");
            //    return BadRequest(ModelState);
            //}

            //_projectScheduleRepository.Delete(record);
            //recordTemplate.ForEach(x =>
            //{
            //    _projectScheduleTemplateRepository.Delete(x);
            //});

            //_uow.Save();

            //_projectScheduleTemplateRepository.UpdateDesignTemplatesSchedule(record.ProjectDesignPeriodId);
            //_uow.Save();

            return Ok();
        }

    }
}