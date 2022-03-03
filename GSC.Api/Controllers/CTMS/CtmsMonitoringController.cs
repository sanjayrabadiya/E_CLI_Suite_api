﻿using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class CtmsMonitoringController : BaseController
    {
        private readonly ICtmsMonitoringRepository _ctmsMonitoringRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public CtmsMonitoringController(ICtmsMonitoringRepository ctmsMonitoringRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _ctmsMonitoringRepository = ctmsMonitoringRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var ctmsMonitoring = _ctmsMonitoringRepository.Find(id);
            var ctmsMonitoringDto = _mapper.Map<CtmsMonitoringDto>(ctmsMonitoring);
            return Ok(ctmsMonitoringDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] CtmsMonitoringDto ctmsMonitoringDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            ctmsMonitoringDto.Id = 0;
            var ctmsMonitoring = _mapper.Map<CtmsMonitoring>(ctmsMonitoringDto);
            _ctmsMonitoringRepository.Add(ctmsMonitoring);
            if (_uow.Save() <= 0) throw new Exception("Creating Monitoring failed on save.");

            return Ok(ctmsMonitoring.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] CtmsMonitoringDto ctmsMonitoringDto)
        {
            if (ctmsMonitoringDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var ctmsMonitoring = _mapper.Map<CtmsMonitoring>(ctmsMonitoringDto);

            _ctmsMonitoringRepository.Update(ctmsMonitoring);
            if (_uow.Save() <= 0) throw new Exception("Updating Monitoring failed on save.");
            return Ok(ctmsMonitoring.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _ctmsMonitoringRepository.Find(id);

            if (record == null)
                return NotFound();

            _ctmsMonitoringRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _ctmsMonitoringRepository.Find(id);

            if (record == null)
                return NotFound();
            _ctmsMonitoringRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        /// Get Monitoring Form list 
        /// Created By Swati
        [HttpGet]
        [Route("GetMonitoringForm/{projectId}/{siteId}/{activityId}")]
        public IActionResult GetMonitoringForm(int projectId, int siteId, int activityId)
        {
            if (projectId <= 0) return BadRequest();

            var result = _ctmsMonitoringRepository.GetMonitoringForm(projectId, siteId, activityId);
            return Ok(result);
        }
    }
}