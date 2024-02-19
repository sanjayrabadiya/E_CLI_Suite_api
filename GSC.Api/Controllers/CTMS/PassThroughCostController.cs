using System;
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
    [ApiController]
    public class PassThroughCostController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IPassThroughCostRepository _passThroughCostRepository;
        private readonly IGSCContext _context;

        public PassThroughCostController(IUnitOfWork uow, IMapper mapper,
            IPassThroughCostRepository passThroughCostRepository, IGSCContext context)
        {
            _uow = uow;
            _mapper = mapper;
            _passThroughCostRepository = passThroughCostRepository;
            _context = context;
        }

        [HttpGet("{isDeleted:bool?}/{studyId:int}")]
        public IActionResult Get(bool isDeleted, int studyId)
        {
            var PassThroughCost = _passThroughCostRepository.GetpassThroughCostGrid(isDeleted, studyId);
            return Ok(PassThroughCost);
        }

        [HttpPost]
        public IActionResult Post([FromBody] PassThroughCostDto passThroughCostDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            passThroughCostDto.Id = 0;
            var passThroughCost = _mapper.Map<PassThroughCost>(passThroughCostDto);
            var validate = _passThroughCostRepository.Duplicate(passThroughCostDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _passThroughCostRepository.ConvertIntoGlobuleCurrency(passThroughCost);
            _passThroughCostRepository.Add(passThroughCost);
            if (_uow.Save() <= 0) throw new Exception("Creating Pass Through Cost failed on save.");
            return Ok(passThroughCost.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] PassThroughCostDto passThroughCostDto)
        {
            if (passThroughCostDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var passThroughCost = _mapper.Map<PassThroughCost>(passThroughCostDto);

            var validate = _passThroughCostRepository.Duplicate(passThroughCostDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _passThroughCostRepository.ConvertIntoGlobuleCurrency(passThroughCost);
            _passThroughCostRepository.AddOrUpdate(passThroughCost);
            if (_uow.Save() <= 0) throw new Exception("Updating Pass Through Cost failed on save.");
            return Ok(passThroughCost.Id);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var record = _passThroughCostRepository.Find(id);
            if (record == null)
                return NotFound();
            _passThroughCostRepository.Delete(record);
            _uow.Save();
            return Ok();
        }

        [HttpGet]
        [Route("GetCountriesDropDown/{projectId:int?}")]
        public IActionResult GetCountriesDropDown(int projectId)
        {
            return Ok(_passThroughCostRepository.GetCountriesDropDown(projectId));
        }
    }
}
