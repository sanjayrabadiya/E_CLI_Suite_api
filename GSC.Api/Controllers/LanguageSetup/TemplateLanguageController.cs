using System;
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
    public class TemplateLanguageController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly ITemplateLanguageRepository _templateLanguageRepository;
        private readonly IUnitOfWork _uow;

        public TemplateLanguageController(
            IUnitOfWork uow, IMapper mapper,
            ITemplateLanguageRepository templateLanguageRepository,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _templateLanguageRepository = templateLanguageRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetTemplateLanguage/{TemplateId}")]
        public IActionResult GetTemplateLanguage(int TemplateId)
        {
            var templateLanguage = _templateLanguageRepository.GetTemplateLanguageList(TemplateId);
            return Ok(templateLanguage);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var templateLanguage = _templateLanguageRepository.Find(id);
            var templateLanguageDto = _mapper.Map<TemplateLanguageDto>(templateLanguage);
            return Ok(templateLanguageDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] TemplateLanguageDto templateLanguageDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            foreach (var item in templateLanguageDto.templateLanguages)
            {
                if (item.Id == 0)
                {
                    TemplateLanguage language = new TemplateLanguage();
                    language.ProjectDesignTemplateId = item.ProjectDesignTemplateId;
                    language.LanguageId = item.LanguageId;
                    language.Display = item.Display;
                    _templateLanguageRepository.Add(language);
                }
            }
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating template language failed on save.");
                return BadRequest(ModelState);
            }
            return Ok();
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _templateLanguageRepository.Find(id);
            if (record == null)
                return NotFound();
            _templateLanguageRepository.Delete(record);
            _uow.Save();
            return Ok();
        }
    }
}
