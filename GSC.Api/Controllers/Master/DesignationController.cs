using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Respository.Master;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class DesignationController : BaseController
    {
        private readonly IDesignationRepository _designationRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public DesignationController(IDesignationRepository designationRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _designationRepository = designationRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var designation = _designationRepository.GetDesignationList(isDeleted);
            return Ok(designation);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var designation = _designationRepository.Find(id);
            var designationDto = _mapper.Map<DesignationDto>(designation);
            return Ok(designationDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] DesignationDto designationDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            designationDto.Id = 0;
            var designation = _mapper.Map<Designation>(designationDto);

            var validate = _designationRepository.Duplicate(designation);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _designationRepository.Add(designation);
            if (_uow.Save() <= 0) throw new Exception("letters Formate failed on save.");
            return Ok(designation.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] DesignationDto designationDto)
        {
            if (designationDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var designation = _mapper.Map<Designation>(designationDto);
            var validate = _designationRepository.Duplicate(designation);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _designationRepository.Update(designation);
            if (_uow.Save() <= 0) throw new Exception("Updating designation on save.");
            return Ok(designation.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _designationRepository.Find(id);

            if (record == null)
                return NotFound();

            _designationRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _designationRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _designationRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _designationRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}