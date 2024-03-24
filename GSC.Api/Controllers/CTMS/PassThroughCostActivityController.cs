using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Respository.CTMS;
using GSC.Data.Entities.CTMS;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class PassThroughCostActivityController : BaseController
    {
        private readonly IPassThroughCostActivityRepository _passThroughCostActivityRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public PassThroughCostActivityController(IPassThroughCostActivityRepository passThroughCostActivityRepository,

            IUnitOfWork uow, IMapper mapper)
        {
            _passThroughCostActivityRepository = passThroughCostActivityRepository;
            _uow = uow;
            _mapper = mapper;
        }
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var PassThroughCostActivity = _passThroughCostActivityRepository.GetPassThroughCostActivityList(isDeleted);
            return Ok(PassThroughCostActivity);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var passThroughCostActivity = _passThroughCostActivityRepository.Find(id);
            var passThroughCostActivityDto = _mapper.Map<PassThroughCostActivityDto>(passThroughCostActivity);
            return Ok(passThroughCostActivityDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] PassThroughCostActivityDto PassThroughCostActivityDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            PassThroughCostActivityDto.Id = 0;
            var PassThroughCostActivity = _mapper.Map<PassThroughCostActivity>(PassThroughCostActivityDto);
            var validate = _passThroughCostActivityRepository.Duplicate(PassThroughCostActivity);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _passThroughCostActivityRepository.Add(PassThroughCostActivity);
            if (_uow.Save() <= 0) throw new Exception("Creating Pass Through Cost Activity failed on save.");
            return Ok(PassThroughCostActivity.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] PassThroughCostActivityDto PassThroughCostActivityDto)
        {
            if (PassThroughCostActivityDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var PassThroughCostActivity = _mapper.Map<PassThroughCostActivity>(PassThroughCostActivityDto);
            var validate = _passThroughCostActivityRepository.Duplicate(PassThroughCostActivity);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _passThroughCostActivityRepository.Update(PassThroughCostActivity);
            if (_uow.Save() <= 0) throw new Exception("Updating Pass Through Cost Activity failed on save.");
            return Ok(PassThroughCostActivity.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _passThroughCostActivityRepository.Find(id);

            if (record == null)
                return NotFound();

            _passThroughCostActivityRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _passThroughCostActivityRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _passThroughCostActivityRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _passThroughCostActivityRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetPassThroughCostActivityDropDown")]
        public IActionResult GetPassThroughCostActivityDropDown()
        {
            return Ok(_passThroughCostActivityRepository.GetPassThroughCostActivityDropDown());
        }
    }
}
