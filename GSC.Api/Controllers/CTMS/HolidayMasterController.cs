﻿using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Respository.CTMS;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class HolidayMasterController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly IHolidayMasterRepository _holidayMasterRepository;

        public HolidayMasterController(IUnitOfWork uow, IMapper mapper,
          IGSCContext context, IHolidayMasterRepository holidayMasterRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _context = context;
            _holidayMasterRepository = holidayMasterRepository;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var holidaylist = _holidayMasterRepository.GetHolidayList(isDeleted);
            return Ok(holidaylist);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var holiday = _holidayMasterRepository.Find(id);
            var holidayDto = _mapper.Map<HolidayMasterDto>(holiday);
            holidayDto.SiteId = holidayDto.IsSite ? holidayDto.ProjectId : (int?)null;
            holidayDto.ProjectId = holidayDto.IsSite ? (int)_context.Project.Find(holidayDto.ProjectId).ParentProjectId : holidayDto.ProjectId;
            return Ok(holidayDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] HolidayMasterDto holidayDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            holidayDto.Id = 0;
            var holiDay = _mapper.Map<HolidayMaster>(holidayDto);

            _holidayMasterRepository.Add(holiDay);
            if (_uow.Save() <= 0) throw new Exception("Creating Holiday failed on save.");
            return Ok(holiDay.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] HolidayMasterDto holidayDto)
        {
            if (holidayDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var holiDay = _mapper.Map<HolidayMaster>(holidayDto);

            _holidayMasterRepository.Update(holiDay);

            if (_uow.Save() <= 0) throw new Exception("Updating holiday failed on save.");
            return Ok(holiDay.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _holidayMasterRepository.Find(id);

            if (record == null)
                return NotFound();

            _holidayMasterRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _holidayMasterRepository.Find(id);

            if (record == null)
                return NotFound();

            _holidayMasterRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetProjectWiseHolidayList/{id}")]
        public ActionResult GetProjectWiseHolidayList(int id)
        {
            var holidaylist = _holidayMasterRepository.GetProjectWiseHolidayList(id);
            return Ok(holidaylist);
        }
    }
}
