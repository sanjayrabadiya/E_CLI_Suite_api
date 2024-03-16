using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Medra;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Medra
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedraVersionController : ControllerBase
    {
        private readonly IMedraVersionRepository _medraVersionRepository;
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public MedraVersionController(IMedraVersionRepository medraVersionRepository,
           IDictionaryRepository dictionaryRepository,
        IUnitOfWork uow,
          IMapper mapper
          )
        {
            _medraVersionRepository = medraVersionRepository;
            _dictionaryRepository = dictionaryRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet]
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var medraversion = _medraVersionRepository.GetMedraVersionList(isDeleted);
            medraversion.ForEach(b =>
            {
                b.Dictionary = _dictionaryRepository.Find(b.DictionaryId);
                b.DictionaryName = _dictionaryRepository.Find(b.DictionaryId).DictionaryName;
            });
            return Ok(medraversion);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            var medra = _medraVersionRepository.Find(id);
            var medraDto = _mapper.Map<MedraVersionDto>(medra);
            return Ok(medraDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] MedraVersionDto medraDto)
        {
            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }
            medraDto.Id = 0;

            var medra = _mapper.Map<MedraVersion>(medraDto);
            var validate = _medraVersionRepository.Duplicate(medra);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _medraVersionRepository.Add(medra);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Medra Version failed on save.");
                return BadRequest(ModelState);                
            }
            return Ok(medra.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] MedraVersionDto medraDto)
        {
            if (medraDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var medra = _mapper.Map<MedraVersion>(medraDto);
            var validate = _medraVersionRepository.Duplicate(medra);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _medraVersionRepository.AddOrUpdate(medra);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Medra Version failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(medra.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _medraVersionRepository.Find(id);

            if (record == null)
                return NotFound();

            _medraVersionRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _medraVersionRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _medraVersionRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _medraVersionRepository.Active(record);
            _uow.Save();
            return Ok();
        }

        [HttpGet]
        [Route("GetMedraVersionDropDown")]
        public IActionResult GetMedraVersionDropDown()
        {
            return Ok(_medraVersionRepository.GetMedraVersionDropDown());
        }

    }
}