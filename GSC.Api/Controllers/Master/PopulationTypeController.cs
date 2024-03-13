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
    public class PopulationTypeController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IPopulationTypeRepository _populationTypeRepository;
        private readonly IUnitOfWork _uow;

        public PopulationTypeController(IPopulationTypeRepository populationTypeRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _populationTypeRepository = populationTypeRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var populationTypes = _populationTypeRepository.GetPopulationTypeList(isDeleted);
            return Ok(populationTypes);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var populationType = _populationTypeRepository.Find(id);
            var populationTypeDto = _mapper.Map<PopulationTypeDto>(populationType);
            return Ok(populationTypeDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] PopulationTypeDto populationTypeDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            populationTypeDto.Id = 0;
            var populationType = _mapper.Map<PopulationType>(populationTypeDto);
            var validate = _populationTypeRepository.Duplicate(populationType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _populationTypeRepository.Add(populationType);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating population type failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(populationType.Id);
        }


        [HttpPut]
        public IActionResult Put([FromBody] PopulationTypeDto populationTypeDto)
        {
            if (populationTypeDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var populationType = _mapper.Map<PopulationType>(populationTypeDto);
            var validate = _populationTypeRepository.Duplicate(populationType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _populationTypeRepository.AddOrUpdate(populationType);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating population type failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(populationType.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _populationTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            _populationTypeRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _populationTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _populationTypeRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _populationTypeRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetPopulationTypeDropDown")]
        public IActionResult GetPopulationTypeDropDown()
        {
            return Ok(_populationTypeRepository.GetPopulationTypeDropDown());
        }
    }
}