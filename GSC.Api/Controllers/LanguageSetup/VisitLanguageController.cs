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
        IJwtTokenAccesser _jwtTokenAccesser;

        public VisitLanguageController(
            IUnitOfWork uow, IMapper mapper,
            IVisitLanguageRepository visitLanguageRepository,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _visitLanguageRepository = visitLanguageRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet]
        [Route("GetVisit/{VisitId}")]
        public IActionResult GetVisit(int VisitId)
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
                VisitLanguage language = new VisitLanguage();
                language.ProjectDesignVisitId = item.ProjectDesignVisitId;
                language.LanguageId = item.LanguageId;
                language.Display = item.Display;
                _visitLanguageRepository.Add(language);
            }
            if (_uow.Save() <= 0) throw new Exception("Creating visit language failed on save.");
            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromBody] VisitLanguageDto visitLanguageDto)
        {
            if (visitLanguageDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            UpdateLevels(visitLanguageDto);
            _uow.Save();
            return Ok();
        }

        private void UpdateLevels(VisitLanguageDto visitLanguageDto)
        {
            var data = _visitLanguageRepository.FindBy(x => x.ProjectDesignVisitId == visitLanguageDto.Id && x.DeletedDate == null);
            foreach (var language in data)
            {
                var languages = _mapper.Map<VisitLanguage>(language);
                languages.DeletedDate = DateTime.Now;
                languages.DeletedBy = _jwtTokenAccesser.UserId;
                _visitLanguageRepository.Update(languages);
            }
            _uow.Save();
            foreach (var language in visitLanguageDto.visitLanguages)
            {
                var languages = _mapper.Map<VisitLanguage>(language);
                languages.Id = 0;
                _visitLanguageRepository.Add(languages);
            }
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
