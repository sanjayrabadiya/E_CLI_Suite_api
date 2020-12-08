using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.LanguageSetup;
using GSC.Data.Entities.LanguageSetup;
using GSC.Respository.LanguageSetup;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.LanguageSetup
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplateNoteLanguageController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly ITemplateNoteLanguageRepository _templateNoteLanguageRepository;
        private readonly IUnitOfWork _uow;
        IJwtTokenAccesser _jwtTokenAccesser;

        public TemplateNoteLanguageController(
            IUnitOfWork uow, IMapper mapper,
            ITemplateNoteLanguageRepository templateNoteLanguageRepository,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _templateNoteLanguageRepository = templateNoteLanguageRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet]
        [Route("GetTemplateNoteLanguage/{TemplateNoteId}")]
        public IActionResult GetTemplateNoteLanguage(int TemplateNoteId)
        {
            var templateNoteLanguage = _templateNoteLanguageRepository.GetTemplateNoteLanguageList(TemplateNoteId);
            return Ok(templateNoteLanguage);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var templateNoteLanguage = _templateNoteLanguageRepository.Find(id);
            var templateNoteLanguageDto = _mapper.Map<TemplateNoteLanguageDto>(templateNoteLanguage);
            return Ok(templateNoteLanguageDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] TemplateNoteLanguageDto templateNoteLanguageDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            foreach (var item in templateNoteLanguageDto.templateNoteLanguages)
            {
                if (item.Id == 0)
                {
                    TemplateNoteLanguage language = new TemplateNoteLanguage();
                    language.ProjectDesignTemplateNoteId = item.ProjectDesignTemplateNoteId;
                    language.LanguageId = item.LanguageId;
                    language.Display = item.Display;
                    _templateNoteLanguageRepository.Add(language);
                }
            }
            if (_uow.Save() <= 0) throw new Exception("Creating template note language failed on save.");
            return Ok();
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _templateNoteLanguageRepository.Find(id);
            if (record == null)
                return NotFound();
            _templateNoteLanguageRepository.Delete(record);
            _uow.Save();
            return Ok();
        }
    }
}
