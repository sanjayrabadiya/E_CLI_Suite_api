﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class TrialTypeController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly ITrialTypeRepository _trialTypeRepository;
        private readonly IUnitOfWork _uow;

        public TrialTypeController(ITrialTypeRepository trialTypeRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _trialTypeRepository = trialTypeRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var trialTypes = _trialTypeRepository.GetTrialTypeList(isDeleted);
            return Ok(trialTypes);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var trialType = _trialTypeRepository.Find(id);
            var trialTypeDto = _mapper.Map<TrialTypeDto>(trialType);
            return Ok(trialTypeDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] TrialTypeDto trialTypeDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            trialTypeDto.Id = 0;
            var trialType = _mapper.Map<TrialType>(trialTypeDto);
            var validate = _trialTypeRepository.Duplicate(trialType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _trialTypeRepository.Add(trialType);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Trial Type failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(trialType.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] TrialTypeDto trialTypeDto)
        {
            if (trialTypeDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var trialType = _mapper.Map<TrialType>(trialTypeDto);
            var validate = _trialTypeRepository.Duplicate(trialType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _trialTypeRepository.AddOrUpdate(trialType);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Trail Type failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(trialType.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _trialTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            _trialTypeRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _trialTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _trialTypeRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _trialTypeRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetTrialTypeDropDown")]
        public IActionResult GetTrialTypeDropDown()
        {
            return Ok(_trialTypeRepository.GetTrialTypeDropDown());
        }
    }
}