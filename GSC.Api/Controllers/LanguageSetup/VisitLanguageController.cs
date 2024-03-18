using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.LanguageSetup;
using GSC.Data.Entities.LanguageSetup;
using GSC.Respository.LanguageSetup;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.LanguageSetup
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisitLanguageController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IVisitLanguageRepository _visitLanguageRepository;
        private readonly IUnitOfWork _uow;

        public VisitLanguageController(
            IUnitOfWork uow, IMapper mapper,
            IVisitLanguageRepository visitLanguageRepository)
        {
            _visitLanguageRepository = visitLanguageRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetVisitLanguage/{VisitId}")]
        public IActionResult GetVisitLanguage(int VisitId)
        {
            var visitLanguage = _visitLanguageRepository.GetVisitLanguageList(VisitId);
            return Ok(visitLanguage);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var visitLanguage = _visitLanguageRepository.Find(id);
            var visitLanguageDto = _mapper.Map<VisitLanguageDto>(visitLanguage);
            return Ok(visitLanguageDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] VisitLanguageDto visitLanguageDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            foreach (var item in visitLanguageDto.visitLanguages)
            {
                if (item.Id == 0)
                {
                    VisitLanguage language = new VisitLanguage();
                    language.ProjectDesignVisitId = item.ProjectDesignVisitId;
                    language.LanguageId = item.LanguageId;
                    language.Display = item.Display;
                    _visitLanguageRepository.Add(language);
                }
            }
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating visit language failed on save.");
                return BadRequest(ModelState);
            }
            return Ok();
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _visitLanguageRepository.Find(id);
            if (record == null)
                return NotFound();
            _visitLanguageRepository.Delete(record);
            _uow.Save();
            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _visitLanguageRepository.Find(id);
            if (record == null)
                return NotFound();
            _visitLanguageRepository.Active(record);
            _uow.Save();
            return Ok();
        }

    }
}
