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
    public class DrugController : BaseController
    {

        private readonly IDrugRepository _drugRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public DrugController(IDrugRepository drugRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _drugRepository = drugRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {

            var drug = _drugRepository.GetDrugList(isDeleted);
            return Ok(drug);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var drug = _drugRepository.Find(id);
            var drugDto = _mapper.Map<DrugDto>(drug);
            return Ok(drugDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] DrugDto drugDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            drugDto.Id = 0;
            var drug = _mapper.Map<Drug>(drugDto);
            var validate = _drugRepository.Duplicate(drug);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _drugRepository.Add(drug);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Drug failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(drug.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] DrugDto drugDto)
        {
            if (drugDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var drug = _mapper.Map<Drug>(drugDto);
            var validate = _drugRepository.Duplicate(drug);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _drugRepository.AddOrUpdate(drug);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Drug failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(drug.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _drugRepository.Find(id);

            if (record == null)
                return NotFound();

            _drugRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _drugRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _drugRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _drugRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetDrugDropDown")]
        public IActionResult GetDrugDropDown()
        {
            return Ok(_drugRepository.GetDrugDropDown());
        }
    }
}