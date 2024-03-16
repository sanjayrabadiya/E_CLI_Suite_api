using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Controllers.Configuration
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanguageConfigurationController : BaseController
    {
        private readonly ILanguageConfigurationRepository _languageConfigurationRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        public LanguageConfigurationController(ILanguageConfigurationRepository languageConfigurationRepository,
           IUnitOfWork uow, IMapper mapper, IGSCContext context)
        {
            _languageConfigurationRepository = languageConfigurationRepository;
            _uow = uow;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var departments = _languageConfigurationRepository.GetlanguageConfiguration(isDeleted);
            return Ok(departments);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var languageconfi = _languageConfigurationRepository.Find(id);
            var languageDto = _mapper.Map<LanguageConfigurationDto>(languageconfi);
            return Ok(languageDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] LanguageConfigurationDto languageconfiDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            languageconfiDto.Id = 0;
            var languageConfi = _mapper.Map<LanguageConfiguration>(languageconfiDto);
            languageConfi.IsReadOnlyDefaultMessage = false;
            var validate = _languageConfigurationRepository.Duplicate(languageConfi);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _languageConfigurationRepository.Add(languageConfi);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Language Configuration failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(languageConfi.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] LanguageConfigurationDto languageconfiDto)
        {
            if (languageconfiDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var languagedetails = _languageConfigurationRepository.Find(languageconfiDto.Id);
            var languageConfi = _mapper.Map<LanguageConfiguration>(languageconfiDto);
            languageConfi.KeyCode = languagedetails.KeyCode;
            languageConfi.IsReadOnlyDefaultMessage = languagedetails.IsReadOnlyDefaultMessage;
            if (languageConfi.IsReadOnlyDefaultMessage)
                languageConfi.DefaultMessage = languagedetails.DefaultMessage;
            var validate = _languageConfigurationRepository.Duplicate(languageConfi);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _languageConfigurationRepository.Update(languageConfi);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Language Configuration failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(languageConfi.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _languageConfigurationRepository.Find(id);

            if (record == null)
                return NotFound();

            _languageConfigurationRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _languageConfigurationRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _languageConfigurationRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _languageConfigurationRepository.Active(record);
            _uow.Save();

            return Ok();
        }


        [HttpGet]
        [Route("GetLanguageDetails/{id}")]
        public IActionResult GetLanguageDetails(int id)
        {
            var details = _languageConfigurationRepository.GetLanguageDetails(id);
            return Ok(details);
        }
        [HttpGet]
        [Route("GetLanguageConfigurationDetails/{id}")]
        public IActionResult GetLanguageConfigurationDetails(int id)
        {
            if (id <= 0) return BadRequest();
            var languageconfi = _context.LanguageConfigurationDetails.Find(id);
            var languageDto = _mapper.Map<LanguageConfigurationDetailsDto>(languageconfi);
            return Ok(languageDto);
        }


        [HttpPost]
        [Route("saveLanguageConfigration")]
        public IActionResult saveLanguageConfigration([FromBody] LanguageConfigurationDetailsDto languageconfiDetailDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            languageconfiDetailDto.Id = 0;
            var languageConfiDetails = _mapper.Map<LanguageConfigurationDetails>(languageconfiDetailDto);
            var validate = _languageConfigurationRepository.DuplicateLanguage(languageConfiDetails);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _context.LanguageConfigurationDetails.Add(languageConfiDetails);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Language Configuration failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(languageConfiDetails.Id);
        }

        [HttpPut]
        [Route("UpdateLanguageConfigration")]
        public IActionResult UpdateLanguageConfigration([FromBody] LanguageConfigurationDetailsDto languageconfiDetailDto)
        {
            if (languageconfiDetailDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var languageConfiDetails = _mapper.Map<LanguageConfigurationDetails>(languageconfiDetailDto);
            var validate = _languageConfigurationRepository.DuplicateLanguage(languageConfiDetails);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _context.LanguageConfigurationDetails.Update(languageConfiDetails);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Language Configuration failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(languageConfiDetails.Id);
        }

        [HttpDelete("DeleteLanguageConfigration/{id}")]
        //[Route("DeleteLanguageConfigration")]
        public ActionResult DeleteLanguageConfigration(int id)
        {
            var record = _context.LanguageConfigurationDetails.Find(id);

            if (record == null)
                return NotFound();

            _context.LanguageConfigurationDetails.Remove(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetMultiLanguage")]
        public ActionResult GetMultiLanguage()
        {
            var details = _languageConfigurationRepository.GetMultiLanguage();
            return Ok(details);
        }
    }
}
