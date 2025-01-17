﻿using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class HolidayController : BaseController
    {
        private readonly IHolidayRepository _holidayRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public HolidayController(IHolidayRepository holidayRepository,
IUnitOfWork uow, IMapper mapper)
        {
            _holidayRepository = holidayRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{id}/{isDeleted:bool?}")]
        public IActionResult Get(int id, bool isDeleted)
        {
            if (id <= 0) return BadRequest();

            var holiday = _holidayRepository.GetHolidayList(id, isDeleted);
            return Ok(holiday);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var holiday = _holidayRepository.Find(id);
            var holidayDto = _mapper.Map<HolidayDto>(holiday);
            return Ok(holidayDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] HolidayDto holidayDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            holidayDto.Id = 0;
            var holiday = _mapper.Map<Holiday>(holidayDto);

            var validate = _holidayRepository.DuplicateHoliday(holiday);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }


            _holidayRepository.Add(holiday);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Investigator holiday failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(holiday.Id);
        }

        // PUT api/<controller>/5
        [HttpPut]
        public IActionResult Put([FromBody] HolidayDto holidayDto)
        {
            if (holidayDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var holiday = _mapper.Map<Holiday>(holidayDto);

            var validate = _holidayRepository.DuplicateHoliday(holiday);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            holiday.Id = holidayDto.Id;

            /* Added by Darshil for effective Date on 21-07-2020 */
            _holidayRepository.AddOrUpdate(holiday);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Investigator holiday failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(holiday.Id);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var record = _holidayRepository.Find(id);

            if (record == null)
                return NotFound();

            _holidayRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public IActionResult Active(int id)
        {
            var record = _holidayRepository.Find(id);

            if (record == null)
                return NotFound();
            _holidayRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetHolidayDropDown")]
        public IActionResult GetHolidayDropDown()
        {
            return Ok(_holidayRepository.GetHolidayDropDown());
        }
    }
}
