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
    public class UnitController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitRepository _unitRepository;
        private readonly IUnitOfWork _uow;

        public UnitController(IUnitRepository unitRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _unitRepository = unitRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var units = _unitRepository.GetUnitList(isDeleted);
            return Ok(units);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var unit = _unitRepository.Find(id);
            var unitDto = _mapper.Map<UnitDto>(unit);
            return Ok(unitDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] UnitDto unitDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            unitDto.Id = 0;
            var unit = _mapper.Map<Unit>(unitDto);
            var validate = _unitRepository.Duplicate(unit);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _unitRepository.Add(unit);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Unit failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(unit.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] UnitDto unitDto)
        {
            if (unitDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var unit = _mapper.Map<Unit>(unitDto);
            var validate = _unitRepository.Duplicate(unit);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _unitRepository.AddOrUpdate(unit);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Unit failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(unit.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _unitRepository.Find(id);

            if (record == null)
                return NotFound();

            _unitRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _unitRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _unitRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _unitRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetUnitDropDown")]
        public IActionResult GetUnitDropDown()
        {
            return Ok(_unitRepository.GetUnitDropDown());
        }
        [HttpGet]
        [Route("GetUnitAsModule/{screenCode}")]
        public IActionResult GetUnitAsModule(string screenCode)
        {
            return Ok(_unitRepository.GetUnitAsModule(screenCode));
        }
    }
}