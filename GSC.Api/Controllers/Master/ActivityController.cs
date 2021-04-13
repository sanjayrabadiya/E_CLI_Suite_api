﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Master;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class ActivityController : BaseController
    {
        private readonly IActivityRepository _activityRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public ActivityController(IActivityRepository activityRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _activityRepository = activityRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var activitysDto = _activityRepository.GetActivityList(isDeleted);
            return Ok(activitysDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var activity = _activityRepository.Find(id);
            var activityDto = _mapper.Map<ActivityDto>(activity);
            return Ok(activityDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] ActivityDto activityDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            activityDto.Id = 0;
            var activity = _mapper.Map<Activity>(activityDto);
            _activityRepository.Add(activity);
            if (_uow.Save() <= 0) throw new Exception("Creating Contact Type failed on save.");

            return Ok(activity.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ActivityDto activityDto)
        {
            if (activityDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var activity = _mapper.Map<Activity>(activityDto);

            _activityRepository.Update(activity);
            if (_uow.Save() <= 0) throw new Exception("Updating Contact Type failed on save.");
            return Ok(activity.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _activityRepository.Find(id);

            if (record == null)
                return NotFound();

            _activityRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _activityRepository.Find(id);

            if (record == null)
                return NotFound();
            _activityRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetActivityDropDown")]
        public IActionResult GetActivityDropDown()
        {
            return Ok(_activityRepository.GetActivityDropDown());
        }
    }
}