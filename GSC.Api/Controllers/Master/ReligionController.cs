using System;
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
    public class ReligionController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IReligionRepository _religionRepository;
        private readonly IUnitOfWork _uow;

        public ReligionController(IReligionRepository religionRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _religionRepository = religionRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var religions = _religionRepository.GetReligionList(isDeleted);
            return Ok(religions);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var religion = _religionRepository.Find(id);
            var religionDto = _mapper.Map<ReligionDto>(religion);
            return Ok(religionDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] ReligionDto religionDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            religionDto.Id = 0;
            var religion = _mapper.Map<Religion>(religionDto);
            var validate = _religionRepository.Duplicate(religion);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _religionRepository.Add(religion);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Religion failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(religion.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ReligionDto religionDto)
        {
            if (religionDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var religion = _mapper.Map<Religion>(religionDto);
            religion.Id = religionDto.Id;
            var validate = _religionRepository.Duplicate(religion);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _religionRepository.AddOrUpdate(religion);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Religion failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(religion.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _religionRepository.Find(id);

            if (record == null)
                return NotFound();

            _religionRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _religionRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _religionRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _religionRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetReligionDropDown")]
        public IActionResult GetReligionDropDown()
        {
            return Ok(_religionRepository.GetReligionDropDown());
        }
    }
}