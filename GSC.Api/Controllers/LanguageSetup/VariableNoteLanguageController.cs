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
    public class VariableNoteLanguageController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IVariabeNoteLanguageRepository _variableNoteLanguageRepository;
        private readonly IUnitOfWork _uow;
        IJwtTokenAccesser _jwtTokenAccesser;

        public VariableNoteLanguageController(
            IUnitOfWork uow, IMapper mapper,
            IVariabeNoteLanguageRepository variableNoteLanguageRepository,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _variableNoteLanguageRepository = variableNoteLanguageRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet]
        [Route("GetVariableNoteLanguage/{VariableId}")]
        public IActionResult GetVariableNoteLanguage(int VariableId)
        {
            var variableNoteLanguage = _variableNoteLanguageRepository.GetVariableNoteLanguageList(VariableId);
            return Ok(variableNoteLanguage);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var variableNoteLanguage = _variableNoteLanguageRepository.Find(id);
            var variableNoteLanguageDto = _mapper.Map<VariableNoteLanguageDto>(variableNoteLanguage);
            return Ok(variableNoteLanguageDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] VariableNoteLanguageDto variableNoteLanguageDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            foreach (var item in variableNoteLanguageDto.variableNoteLanguages)
            {
                if (item.Id == 0)
                {
                    VariableNoteLanguage language = new VariableNoteLanguage();
                    language.ProjectDesignVariableId = item.ProjectDesignVariableId;
                    language.LanguageId = item.LanguageId;
                    language.Display = item.Display;
                    _variableNoteLanguageRepository.Add(language);
                }
            }
            if (_uow.Save() <= 0) throw new Exception("Creating variable note language failed on save.");
            return Ok();
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _variableNoteLanguageRepository.Find(id);
            if (record == null)
                return NotFound();
            _variableNoteLanguageRepository.Delete(record);
            _uow.Save();
            return Ok();
        }
    }
}
